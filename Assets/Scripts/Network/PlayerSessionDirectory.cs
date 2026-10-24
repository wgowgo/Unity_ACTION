using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct SessionSlot : INetworkStruct
{
    public PlayerRef Player;
    public NetworkBool Occupied;
    public NetworkBool Ready;
}

// 로비와 아레나를 오가도 남는 참가자 명단. 전투 캐릭터와 PlayerObject 를 섞지 않는다.
public class PlayerSessionDirectory : NetworkBehaviour
{
    public static PlayerSessionDirectory Instance { get; private set; }

    readonly List<PlayerRef> joinQueue = new List<PlayerRef>();
    readonly List<PlayerRef> leaveQueue = new List<PlayerRef>();
    bool resetReady;

    [Networked] public PlayerRef HostPlayer { get; set; }

    [Networked, Capacity(4)]
    public NetworkArray<SessionSlot> Slots => default;

    public override void Spawned()
    {
        Instance = this;
        if (Object.HasStateAuthority)
        {
            foreach (PlayerRef player in Runner.ActivePlayers)
            {
                Register(player);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        for (int i = 0; i < joinQueue.Count; i++)
        {
            Register(joinQueue[i]);
        }

        joinQueue.Clear();

        for (int i = 0; i < leaveQueue.Count; i++)
        {
            Unregister(leaveQueue[i]);
        }

        leaveQueue.Clear();

        if (resetReady)
        {
            resetReady = false;
            ResetReadyForLobby();
        }
    }

    public void QueueJoin(PlayerRef player)
    {
        if (joinQueue.Contains(player) == false)
        {
            joinQueue.Add(player);
        }
    }

    public void QueueLeave(PlayerRef player)
    {
        if (leaveQueue.Contains(player) == false)
        {
            leaveQueue.Add(player);
        }
    }

    public void QueueLobbyReset()
    {
        resetReady = true;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public int OccupiedCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].Occupied)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public bool AllReady
    {
        get
        {
            int count = 0;
            for (int i = 0; i < Slots.Length; i++)
            {
                SessionSlot slot = Slots[i];
                if (slot.Occupied == false)
                {
                    continue;
                }

                count++;
                if (slot.Ready == false)
                {
                    return false;
                }
            }

            return count >= GameConfig.MinPlayersToStart;
        }
    }

    public void Register(PlayerRef player)
    {
        if (Object.HasStateAuthority == false || player.IsValid == false)
        {
            return;
        }

        int free = -1;
        for (int i = 0; i < Slots.Length; i++)
        {
            SessionSlot slot = Slots[i];
            if (slot.Occupied && slot.Player == player)
            {
                return;
            }

            if (slot.Occupied == false && free < 0)
            {
                free = i;
            }
        }

        if (free < 0)
        {
            return;
        }

        bool host = Runner.IsServer && player == Runner.LocalPlayer;
        if (host)
        {
            HostPlayer = player;
        }

        Slots.Set(free, new SessionSlot
        {
            Player = player,
            Occupied = true,
            Ready = host
        });
    }

    public void Unregister(PlayerRef player)
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        for (int i = 0; i < Slots.Length; i++)
        {
            SessionSlot slot = Slots[i];
            if (slot.Occupied && slot.Player == player)
            {
                Slots.Set(i, default);
            }
        }
    }

    public void ResetReadyForLobby()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        for (int i = 0; i < Slots.Length; i++)
        {
            SessionSlot slot = Slots[i];
            if (slot.Occupied == false)
            {
                continue;
            }

            bool host = Runner.IsServer && slot.Player == Runner.LocalPlayer;
            slot.Ready = host;
            Slots.Set(i, slot);
        }
    }

    public void RequestReady(bool ready)
    {
        if (Object.HasStateAuthority)
        {
            SetReady(Runner.LocalPlayer, ready);
        }
        else
        {
            RPC_SetReady(ready);
        }
    }

    public bool LocalReady
    {
        get
        {
            if (Runner == null)
            {
                return false;
            }

            for (int i = 0; i < Slots.Length; i++)
            {
                SessionSlot slot = Slots[i];
                if (slot.Occupied && slot.Player == Runner.LocalPlayer)
                {
                    return slot.Ready;
                }
            }

            return false;
        }
    }

    public string BuildRoster(NetworkRunner runner)
    {
        string text = "";
        for (int i = 0; i < Slots.Length; i++)
        {
            SessionSlot slot = Slots[i];
            if (slot.Occupied == false)
            {
                continue;
            }

            string role = slot.Player == HostPlayer ? "Host" : "Client";

            string ready = slot.Ready ? "Ready" : "Wait";
            text += "P" + slot.Player.PlayerId + "  " + role + "  " + ready + "\n";
        }

        return text;
    }

    void SetReady(PlayerRef player, bool ready)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            SessionSlot slot = Slots[i];
            if (slot.Occupied && slot.Player == player)
            {
                slot.Ready = ready;
                Slots.Set(i, slot);
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool ready, RpcInfo info = default)
    {
        SetReady(info.Source, ready);
    }
}
