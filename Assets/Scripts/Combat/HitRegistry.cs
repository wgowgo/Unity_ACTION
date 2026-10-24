using Fusion;

// 같은 공격 일련번호가 같은 대상에게 다시 들어가지 않게 막는다. 값은 네트워크 상태라 재시뮬레이션에도 되돌아간다.
public static class HitRegistry
{
    public static bool AlreadyLanded(PlayerAvatar victim, PlayerAvatar attacker)
    {
        bool incoming = victim.IncomingSerial == attacker.AttackSerial && victim.IncomingFrom == attacker.Object.InputAuthority;
        bool consumed = victim.ConsumedSerial == attacker.AttackSerial && victim.ConsumedFrom == attacker.Object.InputAuthority;
        return incoming || consumed;
    }

    public static void Mark(PlayerAvatar victim, PlayerAvatar attacker, DamageResult result)
    {
        victim.IncomingSerial = attacker.AttackSerial;
        victim.IncomingFrom = attacker.Object.InputAuthority;
        victim.IncomingDamage = result.Damage;
        victim.IncomingStunTicks = result.StunTicks;
        victim.IncomingKnock = result.KnockDirection;
        victim.IncomingKnockStrength = result.KnockStrength;
        victim.IncomingParry = result.Parried;
        victim.IncomingGuard = result.Guarded;
        victim.IncomingLaunch = result.Launch;
    }
}
