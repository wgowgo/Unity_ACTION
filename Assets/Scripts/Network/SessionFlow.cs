using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

// 방 입장 이후의 호스트 흐름: 명단 유지, 경기 중 입장 거부, 아레나 스폰, 퇴장 정리.
public class SessionFlow : RunnerCallbacks
{
    public static bool IntentionalLeave;
    public static string Notice;
    public static string SpawnError;

    public const string PlayerPrefab = "Network/PlayerAvatar";
    public const string SessionPrefab = "Network/SessionDirectory";
    public const string MatchPrefab = "Network/MatchState";

    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer == false)
        {
            return;
        }

        EnsureDirectory(runner);
        if (PlayerSessionDirectory.Instance != null)
        {
            PlayerSessionDirectory.Instance.QueueJoin(player);
        }
    }

    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer == false)
        {
            return;
        }

        PlayerAvatar avatar = PlayerAvatar.Find(player);
        if (avatar != null)
        {
            runner.Despawn(avatar.Object);
        }

        if (PlayerSessionDirectory.Instance != null)
        {
            PlayerSessionDirectory.Instance.QueueLeave(player);
        }

        if (MatchStateManager.Active != null)
        {
            MatchStateManager.Active.RequestSurvivorCheck();
        }
    }

    public override void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        bool inMatch = MatchStateManager.Active != null;
        bool full = runner.SessionInfo.IsValid && runner.SessionInfo.PlayerCount >= NetworkManager.MAX_PLAYERS;
        if (inMatch || full)
        {
            request.Refuse();
            return;
        }

        request.Accept();
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        string scene = SceneManager.GetActiveScene().name;
        if (runner.IsServer == false)
        {
            if (scene == SceneNames.Play)
            {
                ArenaGeometry.Ensure();
            }

            return;
        }

        if (scene == SceneNames.Lobby)
        {
            if (PlayerSessionDirectory.Instance != null)
            {
                PlayerSessionDirectory.Instance.QueueLobbyReset();
            }

            return;
        }

        if (scene != SceneNames.Play)
        {
            return;
        }

        ArenaGeometry.Ensure();
        SpawnMatch(runner);
    }

    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        string scene = SceneManager.GetActiveScene().name;
        bool leavingPlay = scene == SceneNames.Play || scene == SceneNames.Loading;
        if (leavingPlay == false)
        {
            IntentionalLeave = false;
            return;
        }

        if (IntentionalLeave == false && shutdownReason != ShutdownReason.Ok)
        {
            Notice = "Disconnected. (" + shutdownReason + ")";
        }

        IntentionalLeave = false;
        SceneManager.LoadScene(SceneNames.Lobby);
    }

    public static void LeaveMatch()
    {
        IntentionalLeave = true;
        if (NetworkManager.Instance != null)
        {
            _ = NetworkManager.Instance.LeaveRoom();
        }
    }

    void EnsureDirectory(NetworkRunner runner)
    {
        if (PlayerSessionDirectory.Instance != null)
        {
            return;
        }

        NetworkObject prefab = Resources.Load<NetworkObject>(SessionPrefab);
        if (prefab == null)
        {
            Debug.LogError("Session prefab is missing. Run Tools/Action Game/Build Combat Content.");
            return;
        }

        NetworkObject spawned = runner.Spawn(prefab);
        if (spawned == null)
        {
            return;
        }

        spawned.transform.SetParent(runner.transform);
        DontDestroyOnLoad(spawned.gameObject);
        PinInterest(runner, spawned);
    }

    void SpawnMatch(NetworkRunner runner)
    {
        if (PlayerAvatar.All.Count > 0)
        {
            return;
        }

        NetworkObject matchPrefab = Resources.Load<NetworkObject>(MatchPrefab);
        NetworkObject playerPrefab = Resources.Load<NetworkObject>(PlayerPrefab);
        if (matchPrefab == null || playerPrefab == null)
        {
            SpawnError = "Combat prefabs are missing. Run Tools/Action Game/Build Combat Content, then start again.";
            Debug.LogError(SpawnError);
            return;
        }

        SpawnError = null;

        if (MatchStateManager.Active == null)
        {
            NetworkObject matchObject = runner.Spawn(matchPrefab);
            PinInterest(runner, matchObject);
        }

        PlayerSessionDirectory directory = PlayerSessionDirectory.Instance;
        if (directory == null)
        {
            EnsureDirectory(runner);
            directory = PlayerSessionDirectory.Instance;
        }

        var players = new List<PlayerRef>();
        if (directory != null)
        {
            for (int i = 0; i < directory.Slots.Length; i++)
            {
                SessionSlot slot = directory.Slots[i];
                if (slot.Occupied)
                {
                    players.Add(slot.Player);
                }
            }
        }
        if (players.Count == 0)
        {
            foreach (PlayerRef player in runner.ActivePlayers)
            {
                players.Add(player);
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            int spawnIndex = i;
            PlayerRef player = players[i];
            Vector3 position = GameConfig.SpawnPoint(spawnIndex);
            Quaternion rotation = GameConfig.SpawnRotation(spawnIndex);
            NetworkObject avatarObject = runner.Spawn(playerPrefab, position, rotation, player, (_, obj) =>
            {
                PlayerAvatar avatar = obj.GetComponent<PlayerAvatar>();
                if (avatar != null)
                {
                    avatar.InitializeSpawn(spawnIndex);
                }
            });
            PinInterest(runner, avatarObject);
        }
    }

    static void PinInterest(NetworkRunner runner, NetworkObject obj)
    {
        if (obj == null || runner.IsServer == false)
        {
            return;
        }

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            runner.SetPlayerAlwaysInterested(player, obj, true);
        }
    }
}
