using UnityEngine;

// 학생 튜닝용 에셋. 비어 있으면 CombatBalance 가 코드 기본값을 채운다.
[CreateAssetMenu(fileName = "CombatBalance", menuName = "Action/Combat Balance")]
public class CombatBalanceConfig : ScriptableObject
{
    public float MaxHealth = 100f;
    public float UltimateCost = 100f;
    public float GuardDamageScale = 0.4f;
    public float ParrySeconds = 0.12f;
    public float DashActive = 0.2f;
    public float DashRecovery = 0.1f;
    public float DashCooldown = 1.5f;
    public float CounterWindow = 0.8f;
    public float MatchSeconds = 180f;

    public AttackProfile Light1;
    public AttackProfile Light2;
    public AttackProfile Light3;
    public AttackProfile Counter;
    public AttackProfile Ultimate;
    public AttackProfile Heavy;

    public void ApplyDefaults()
    {
        MaxHealth = GameConfig.MaxHealth;
        UltimateCost = GameConfig.UltimateMax;
        GuardDamageScale = 0.4f;
        ParrySeconds = 0.12f;
        DashActive = 0.2f;
        DashRecovery = 0.1f;
        DashCooldown = 1.5f;
        CounterWindow = GameConfig.CounterWindowSeconds;
        MatchSeconds = GameConfig.MatchSeconds;

        Light1 = New(10f, 0.15f, 0.10f, 0.20f, 2.5f, 0.28f, 1.15f, 20f);
        Light2 = New(12f, 0.18f, 0.12f, 0.25f, 3.2f, 0.32f, 1.25f, 20f);
        Light3 = New(20f, 0.25f, 0.15f, 0.40f, 6.5f, 0.48f, 1.45f, 25f);
        Counter = New(25f, 0.20f, 0.12f, 0.40f, 5.5f, 0.42f, 1.35f, 15f);
        Ultimate = New(50f, 0.50f, 0.25f, 0.80f, 8f, 0.70f, 2.1f, 0f);
        Heavy = New(18f, 0.22f, 0.12f, 0.36f, 4.8f, 0.46f, 1.4f, 18f);
        Light1 = New(8f, 5f / 60f, 3f / 60f, 10f / 60f, 1.6f, 0.22f, 1.15f, 12f);
    }

    static AttackProfile New(float damage, float startup, float active, float recovery, float knock, float stun, float range, float gauge)
    {
        return new AttackProfile
        {
            Damage = damage,
            Startup = startup,
            Active = active,
            Recovery = recovery,
            Knockback = knock,
            HitStun = stun,
            Range = range,
            GaugeGain = gauge
        };
    }
}
