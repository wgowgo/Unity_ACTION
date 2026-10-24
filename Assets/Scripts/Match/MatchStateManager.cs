using Fusion;
using UnityEngine;

// 99초 라운드와 2선승. KO, 시간 종료, 무승부를 호스트가 확정한다.
public class MatchStateManager : NetworkBehaviour
{
    public static MatchStateManager Active { get; private set; }

    bool rematchRequested;
    bool lobbyRequested;
    bool survivorsDirty;

    [Networked] public MatchPhase Phase { get; set; }
    [Networked] public int RoundId { get; set; }
    [Networked] public TickTimer PhaseTimer { get; set; }
    [Networked] public PlayerRef Winner { get; set; }
    [Networked] public NetworkBool IsDraw { get; set; }
    [Networked] public int LeftWins { get; set; }
    [Networked] public int RightWins { get; set; }
    [Networked] public int DisplayRound { get; set; }
    [Networked] public NetworkBool MatchDecided { get; set; }
    [Networked] public RoundEndReason EndReason { get; set; }

    public override void Spawned()
    {
        Active = this;
        if (Object.HasStateAuthority)
        {
            BeginCountdown();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Active == this)
        {
            Active = null;
        }
    }

    public void BeginCountdown()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        Winner = PlayerRef.None;
        IsDraw = false;
        EndReason = RoundEndReason.None;
        if (DisplayRound <= 0)
        {
            DisplayRound = 1;
        }

        Phase = MatchPhase.Countdown;
        PhaseTimer = TickTimer.CreateFromSeconds(Runner, GameConfig.CountdownSeconds);
    }

    public void RequestSurvivorCheck()
    {
        survivorsDirty = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        if (rematchRequested)
        {
            rematchRequested = false;
            if (Phase == MatchPhase.Results)
            {
                ServerRematch();
            }

            return;
        }

        if (lobbyRequested)
        {
            lobbyRequested = false;
            if (Phase == MatchPhase.Results || Phase == MatchPhase.Finishing)
            {
                ServerLobby();
            }

            return;
        }

        if (survivorsDirty)
        {
            survivorsDirty = false;
            EvaluateSurvivors();
        }

        if (Phase == MatchPhase.Countdown && PhaseTimer.Expired(Runner))
        {
            Phase = MatchPhase.Playing;
            PhaseTimer = TickTimer.CreateFromSeconds(Runner, GameConfig.RoundSeconds);
        }
        else if (Phase == MatchPhase.Playing)
        {
            EvaluateSurvivors();
            if (Phase == MatchPhase.Playing && PhaseTimer.Expired(Runner))
            {
                ResolveByHealth();
            }
        }
        else if (Phase == MatchPhase.Finishing && PhaseTimer.Expired(Runner))
        {
            if (MatchDecided)
            {
                Phase = MatchPhase.Results;
            }
            else
            {
                DisplayRound += 1;
                for (int i = 0; i < PlayerAvatar.All.Count; i++)
                {
                    PlayerAvatar.All[i].ResetForRound();
                }

                BeginCountdown();
            }
        }
    }

    public void EvaluateSurvivors()
    {
        if (Object.HasStateAuthority == false || Phase != MatchPhase.Playing)
        {
            return;
        }

        int total = PlayerAvatar.All.Count;
        if (total < 2)
        {
            return;
        }

        int alive = 0;
        PlayerRef last = PlayerRef.None;
        for (int i = 0; i < PlayerAvatar.All.Count; i++)
        {
            PlayerAvatar avatar = PlayerAvatar.All[i];
            if (avatar.Health > 0f)
            {
                alive++;
                last = avatar.Object.InputAuthority;
            }
        }

        if (alive <= 0)
        {
            EndRound(PlayerRef.None, true, RoundEndReason.Draw);
        }
        else if (alive == 1 && total >= 2)
        {
            EndRound(last, false, RoundEndReason.KO);
        }
    }

    public void RequestRematch()
    {
        if (Phase != MatchPhase.Results)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            rematchRequested = true;
        }
        else
        {
            RPC_RequestRematch();
        }
    }

    public void RequestLobby()
    {
        if (Phase != MatchPhase.Results && Phase != MatchPhase.Finishing)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            lobbyRequested = true;
        }
        else
        {
            RPC_RequestLobby();
        }
    }

    void ResolveByHealth()
    {
        PlayerAvatar best = null;
        bool tie = false;
        for (int i = 0; i < PlayerAvatar.All.Count; i++)
        {
            PlayerAvatar avatar = PlayerAvatar.All[i];
            if (avatar.Health <= 0f)
            {
                continue;
            }

            if (best == null)
            {
                best = avatar;
                continue;
            }

            float bestRatio = best.Health / Mathf.Max(1f, best.MaxHealth);
            float ratio = avatar.Health / Mathf.Max(1f, avatar.MaxHealth);
            if (Mathf.Abs(bestRatio - ratio) < 0.01f)
            {
                tie = true;
            }
            else if (ratio > bestRatio)
            {
                best = avatar;
                tie = false;
            }
        }

        if (best == null || tie)
        {
            EndRound(PlayerRef.None, true, RoundEndReason.Draw);
        }
        else
        {
            EndRound(best.Object.InputAuthority, false, RoundEndReason.Time);
        }
    }

    void EndRound(PlayerRef winner, bool draw, RoundEndReason reason)
    {
        if (Phase != MatchPhase.Playing)
        {
            return;
        }

        if (draw == false)
        {
            PlayerAvatar avatar = PlayerAvatar.Find(winner);
            if (avatar != null && avatar.SpawnIndex == 0)
            {
                LeftWins += 1;
            }
            else if (avatar != null)
            {
                RightWins += 1;
            }
        }

        Winner = winner;
        IsDraw = draw;
        EndReason = reason;
        MatchDecided = LeftWins >= GameConfig.RoundsToWin || RightWins >= GameConfig.RoundsToWin;
        Phase = MatchPhase.Finishing;
        PhaseTimer = TickTimer.CreateFromSeconds(Runner, GameConfig.FinishDelaySeconds);
    }

    void ServerRematch()
    {
        RoundId++;
        LeftWins = 0;
        RightWins = 0;
        DisplayRound = 1;
        MatchDecided = false;
        Winner = PlayerRef.None;
        IsDraw = false;
        for (int i = 0; i < PlayerAvatar.All.Count; i++)
        {
            PlayerAvatar.All[i].ResetForRound();
        }

        Phase = MatchPhase.Countdown;
        PhaseTimer = TickTimer.CreateFromSeconds(Runner, GameConfig.CountdownSeconds);
    }

    void ServerLobby()
    {
        if (PlayerSessionDirectory.Instance != null)
        {
            PlayerSessionDirectory.Instance.ResetReadyForLobby();
        }

        Runner.LoadScene(SceneNames.Lobby);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestRematch()
    {
        rematchRequested = true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestLobby()
    {
        lobbyRequested = true;
    }
}
