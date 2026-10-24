using Fusion;
using UnityEngine;

// 실행 중인 밸런스. 에셋이 있으면 그 값을, 없으면 명세의 초기값을 쓴다.
public static class CombatBalance
{
    static CombatBalanceConfig active;

    public static CombatBalanceConfig Active
    {
        get
        {
            if (active == null)
            {
                active = Resources.Load<CombatBalanceConfig>("Network/CombatBalance");
                if (active == null)
                {
                    active = ScriptableObject.CreateInstance<CombatBalanceConfig>();
                    active.ApplyDefaults();
                }
            }

            return active;
        }
    }

    static AttackProfile HeavyFallback()
    {
        return new AttackProfile
        {
            Damage = 18f,
            Startup = 0.22f,
            Active = 0.12f,
            Recovery = 0.36f,
            Knockback = 4.8f,
            HitStun = 0.46f,
            Range = 1.4f,
            GaugeGain = 18f
        };
    }

    public static int Ticks(NetworkRunner runner, float seconds)
    {
        if (runner == null || runner.TickRate <= 0)
        {
            return Mathf.Max(1, Mathf.RoundToInt(seconds * 60f));
        }

        return Mathf.Max(1, Mathf.RoundToInt(seconds * runner.TickRate));
    }

    public static AttackProfile Get(AttackId id)
    {
        CombatBalanceConfig config = Active;
        switch (id)
        {
            case AttackId.Light2: return config.Light2;
            case AttackId.Light3: return config.Light3;
            case AttackId.Counter: return config.Counter;
            case AttackId.Ultimate: return config.Ultimate;
            case AttackId.Heavy:
                return config.Heavy.Damage > 0f ? config.Heavy : HeavyFallback();
            default: return config.Light1;
        }
    }
}
