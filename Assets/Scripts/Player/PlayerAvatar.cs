using System.Collections.Generic;
using Fusion;
using UnityEngine;

// 전투 캐릭터의 확정 상태. 시뮬레이션 틱은 이동 다음 전투 순서로 한 번만 돈다.
public class PlayerAvatar : NetworkBehaviour
{
    public static readonly List<PlayerAvatar> All = new List<PlayerAvatar>();

    [Networked] public ActionState Action { get; set; }
    [Networked] public MovementState Movement { get; set; }
    [Networked] public int SpawnIndex { get; set; }
    [Networked] public float Health { get; set; }
    [Networked] public float MaxHealth { get; set; }
    [Networked] public float Gauge { get; set; }
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }
    [Networked] public int AttackSerial { get; set; }
    [Networked] public int ComboStep { get; set; }
    [Networked] public NetworkBool ComboQueued { get; set; }
    [Networked] public int ActionStartTick { get; set; }
    [Networked] public int ActionEndTick { get; set; }
    [Networked] public int ActiveStartTick { get; set; }
    [Networked] public int ActiveEndTick { get; set; }
    [Networked] public int ComboOpenTick { get; set; }
    [Networked] public int ComboCloseTick { get; set; }
    [Networked] public int CancelOpenTick { get; set; }
    [Networked] public AttackId CurrentAttack { get; set; }
    [Networked] public sbyte Facing { get; set; }
    [Networked] public NetworkBool IsCrouching { get; set; }
    [Networked] public AttackHeight SwingHeight { get; set; }
    [Networked] public NetworkBool IsGuarding { get; set; }
    [Networked] public int GuardStartTick { get; set; }
    [Networked] public TickTimer CounterWindow { get; set; }
    [Networked] public TickTimer DashCooldown { get; set; }
    [Networked] public int DashEndTick { get; set; }
    [Networked] public Vector3 DashDirection { get; set; }
    [Networked] public int HitStunEndTick { get; set; }
    [Networked] public Vector3 KnockDirection { get; set; }
    [Networked] public float KnockStrength { get; set; }
    [Networked] public int AttackBufferTick { get; set; }
    [Networked] public int HeavyBufferTick { get; set; }
    [Networked] public int DashBufferTick { get; set; }
    [Networked] public int JumpBufferTick { get; set; }
    [Networked] public int CounterBufferTick { get; set; }
    [Networked] public int UltimateBufferTick { get; set; }
    [Networked] public int IncomingSerial { get; set; }
    [Networked] public PlayerRef IncomingFrom { get; set; }
    [Networked] public float IncomingDamage { get; set; }
    [Networked] public int IncomingStunTicks { get; set; }
    [Networked] public Vector3 IncomingKnock { get; set; }
    [Networked] public float IncomingKnockStrength { get; set; }
    [Networked] public NetworkBool IncomingParry { get; set; }
    [Networked] public NetworkBool IncomingGuard { get; set; }
    [Networked] public NetworkBool IncomingLaunch { get; set; }
    [Networked] public int ConsumedSerial { get; set; }
    [Networked] public PlayerRef ConsumedFrom { get; set; }

    public NetworkCharacterController Character { get; private set; }
    public Transform Body { get; private set; }
    public Transform Weapon { get; private set; }
    public Transform AttackOrigin { get; private set; }

    readonly PlayerMotor motor = new PlayerMotor();
    readonly PlayerCombatController combat = new PlayerCombatController();
    readonly PlayerAnimationPresenter presenter = new PlayerAnimationPresenter();

    public static PlayerAvatar Find(PlayerRef player)
    {
        for (int i = 0; i < All.Count; i++)
        {
            PlayerAvatar avatar = All[i];
            if (avatar != null && avatar.Object != null && avatar.Object.InputAuthority == player)
            {
                return avatar;
            }
        }

        return null;
    }

    public override void Spawned()
    {
        if (All.Contains(this) == false)
        {
            All.Add(this);
        }

        Character = GetComponent<NetworkCharacterController>();
        EnsureVisuals();
        AttachDummy();
        presenter.OnSpawned(this);
        gameObject.name = "Player " + Object.InputAuthority.PlayerId;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        All.Remove(this);
        presenter.OnDespawned(this);
    }

    public void InitializeSpawn(int index)
    {
        SpawnIndex = index;
        MaxHealth = CombatBalance.Active.MaxHealth;
        Health = MaxHealth;
        Gauge = 0f;
        Kills = 0;
        Deaths = 0;
        Action = ActionState.None;
        Movement = MovementState.Idle;
        Facing = (sbyte)(index == 0 ? 1 : -1);
        IsCrouching = false;
        ClearAttack();
    }

    public PlayerAvatar Opponent()
    {
        PlayerAvatar nearest = null;
        float best = float.MaxValue;
        for (int i = 0; i < All.Count; i++)
        {
            PlayerAvatar other = All[i];
            if (other == null || other == this)
            {
                continue;
            }

            float distance = Mathf.Abs(other.transform.position.x - transform.position.x);
            if (distance < best)
            {
                best = distance;
                nearest = other;
            }
        }

        return nearest;
    }

    public void ResetForRound()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        MaxHealth = CombatBalance.Active.MaxHealth;
        Health = MaxHealth;
        Gauge = 0f;
        Action = ActionState.None;
        Movement = MovementState.Idle;
        IsGuarding = false;
        IsCrouching = false;
        IncomingLaunch = false;
        CounterWindow = TickTimer.None;
        DashCooldown = TickTimer.None;
        HitStunEndTick = 0;
        KnockStrength = 0f;
        AttackBufferTick = 0;
        HeavyBufferTick = 0;
        DashBufferTick = 0;
        JumpBufferTick = 0;
        CounterBufferTick = 0;
        UltimateBufferTick = 0;
        IncomingSerial = 0;
        ConsumedSerial = 0;
        ClearAttack();
        PlayerSpawner.Place(this, SpawnIndex);
        PlayerMotor.LockLane(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false && Object.HasInputAuthority == false)
        {
            return;
        }

        if (GetInput(out CombatNetworkInput input) == false)
        {
            input = default;
        }

        combat.Tick(this, input);
        motor.Tick(this, input);
    }

    public override void Render()
    {
        presenter.Render(this);
    }

    public bool Playing
    {
        get
        {
            MatchStateManager match = MatchStateManager.Active;
            return match != null && match.Phase == MatchPhase.Playing;
        }
    }

    public bool BufferFresh(int tick)
    {
        if (tick <= 0 || Runner == null)
        {
            return false;
        }

        return Runner.Tick - tick <= CombatBalance.Ticks(Runner, GameConfig.InputBufferSeconds);
    }

    public void ClearAttack()
    {
        ComboStep = 0;
        ComboQueued = false;
        ActionStartTick = 0;
        ActionEndTick = 0;
        ActiveStartTick = 0;
        ActiveEndTick = 0;
        ComboOpenTick = 0;
        ComboCloseTick = 0;
        CancelOpenTick = 0;
    }

    public void AddGauge(float amount)
    {
        if (Object.HasStateAuthority == false || amount <= 0f)
        {
            return;
        }

        Gauge = Mathf.Clamp(Gauge + amount, 0f, GameConfig.UltimateMax);
    }

    void EnsureVisuals()
    {
        Body = transform.Find("Body");
        Weapon = transform.Find("Weapon");
        AttackOrigin = transform.Find("AttackOrigin");
        if (Body != null && Weapon != null && AttackOrigin != null)
        {
            return;
        }

        if (Body == null)
        {
            Body = CreatePrimitive("Body", PrimitiveType.Capsule, new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f));
        }

        if (Weapon == null)
        {
            Weapon = CreatePrimitive("Weapon", PrimitiveType.Cube, new Vector3(0.35f, 1.15f, 0.45f), new Vector3(0.12f, 0.12f, 0.85f));
        }

        if (AttackOrigin == null)
        {
            var origin = new GameObject("AttackOrigin");
            origin.transform.SetParent(transform, false);
            origin.transform.localPosition = new Vector3(0f, 1.1f, 0.9f);
            AttackOrigin = origin.transform;
        }
    }

    void AttachDummy()
    {
        if (transform.Find("FighterDummy") != null)
        {
            return;
        }

        FighterDummySet set = Resources.Load<FighterDummySet>("Network/FighterDummies");
        if (set == null)
        {
            return;
        }

        GameObject prefab = SpawnIndex == 0 ? set.PlayerOne : set.PlayerTwo;
        if (prefab == null)
        {
            prefab = set.PlayerOne != null ? set.PlayerOne : set.PlayerTwo;
        }

        if (prefab == null)
        {
            return;
        }

        GameObject dummy = UnityEngine.Object.Instantiate(prefab, transform);
        dummy.name = "FighterDummy";
        dummy.transform.localPosition = Vector3.zero;
        dummy.transform.localRotation = Quaternion.identity;
        dummy.transform.localScale = Vector3.one;
        Collider[] colliders = dummy.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            UnityEngine.Object.Destroy(colliders[i]);
        }

        Rigidbody[] bodies = dummy.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            UnityEngine.Object.Destroy(bodies[i]);
        }

        CharacterController[] controllers = dummy.GetComponentsInChildren<CharacterController>(true);
        for (int i = 0; i < controllers.Length; i++)
        {
            UnityEngine.Object.Destroy(controllers[i]);
        }

        if (Body != null)
        {
            Renderer renderer = Body.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        if (Weapon != null)
        {
            Renderer renderer = Weapon.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    Transform CreatePrimitive(string objectName, PrimitiveType type, Vector3 localPosition, Vector3 scale)
    {
        var primitive = GameObject.CreatePrimitive(type);
        primitive.name = objectName;
        primitive.transform.SetParent(transform, false);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localScale = scale;
        Collider collider = primitive.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return primitive.transform;
    }
}
