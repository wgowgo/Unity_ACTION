using System;
using UnityEngine;

public enum ActionButton
{
    Jump,
    Attack,
    Guard,
    Dash,
    Counter,
    Ultimate,
    Crouch,
    Heavy
}

public enum AttackHeight : byte
{
    High,
    Mid,
    Low
}

public enum RoundEndReason : byte
{
    None,
    KO,
    Time,
    Draw
}

public enum MovementState : byte
{
    Idle,
    Move,
    Jump,
    Fall,
    Dash,
    Knockback
}

public enum ActionState : byte
{
    None,
    Attack,
    Guard,
    Dash,
    Counter,
    Ultimate,
    HitStun,
    Dead
}

public enum MatchPhase : byte
{
    Waiting,
    Countdown,
    Playing,
    Finishing,
    Results
}

public enum AttackId
{
    Light1,
    Light2,
    Light3,
    Counter,
    Ultimate,
    Heavy
}

// 공격 하나의 수치. 초 단위로 적고, 실행 시 틱으로 바꾼다.
[Serializable]
public struct AttackProfile
{
    public float Damage;
    public float Startup;
    public float Active;
    public float Recovery;
    public float Knockback;
    public float HitStun;
    public float Range;
    public float GaugeGain;

    public float Total => Startup + Active + Recovery;
}
