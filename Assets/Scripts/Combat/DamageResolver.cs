using UnityEngine;

public struct DamageResult
{
    public float Damage;
    public bool Parried;
    public bool Guarded;
    public bool Whiffed;
    public bool Launch;
    public int StunTicks;
    public Vector3 KnockDirection;
    public float KnockStrength;
    public bool OpenCounter;
}

// 공격 높이와 가드 자세를 먼저 보고, 그 다음 패링과 피해를 정한다.
public static class DamageResolver
{
    public static DamageResult Resolve(PlayerAvatar attacker, PlayerAvatar victim, AttackProfile profile)
    {
        var result = new DamageResult
        {
            Damage = profile.Damage,
            StunTicks = CombatBalance.Ticks(attacker.Runner, profile.HitStun),
            KnockStrength = profile.Knockback
        };

        Vector3 knock = victim.transform.position - attacker.transform.position;
        knock.y = 0f;
        knock.z = 0f;
        if (knock.sqrMagnitude < 0.001f)
        {
            knock = new Vector3(attacker.Facing >= 0 ? 1f : -1f, 0f, 0f);
        }

        result.KnockDirection = knock.normalized;

        AttackHeight height = attacker.SwingHeight;
        bool crouching = victim.IsCrouching;
        if (height == AttackHeight.High && crouching)
        {
            result.Damage = 0f;
            result.Whiffed = true;
            result.StunTicks = 0;
            result.KnockStrength = 0f;
            return result;
        }

        bool heightBlocked = Blocks(height, crouching);
        int elapsed = victim.Runner.Tick - victim.GuardStartTick;
        int parryTicks = CombatBalance.Ticks(victim.Runner, CombatBalance.Active.ParrySeconds);
        bool canParry = height != AttackHeight.Low && crouching == false;
        bool parry = canParry && victim.IsGuarding && elapsed >= 0 && elapsed <= parryTicks;

        if (parry)
        {
            result.Damage = 0f;
            result.Parried = true;
            result.StunTicks = 0;
            result.KnockStrength = 0f;
            result.OpenCounter = true;
            return result;
        }

        if (victim.IsGuarding && heightBlocked)
        {
            result.Damage *= CombatBalance.Active.GuardDamageScale;
            result.Guarded = true;
            result.StunTicks = CombatBalance.Ticks(victim.Runner, 0.16f);
            result.KnockStrength *= 0.3f;
            result.OpenCounter = true;
            return result;
        }

        result.Launch = attacker.CurrentAttack == AttackId.Heavy && height == AttackHeight.Mid;
        return result;
    }

    static bool Blocks(AttackHeight height, bool crouching)
    {
        if (height == AttackHeight.Low)
        {
            return crouching;
        }

        if (height == AttackHeight.Mid)
        {
            return crouching == false;
        }

        return crouching == false;
    }
}
