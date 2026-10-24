using System.Collections.Generic;
using UnityEngine;

// 같은 틱의 X축 거리와 바라보는 방향으로 맞았는지 본다. 공격과 방어를 같은 틱에서 해석한다.
public static class HitDetectionService
{
    static readonly List<PlayerAvatar> Targets = new List<PlayerAvatar>(8);

    public static void Resolve(PlayerAvatar attacker, AttackProfile profile)
    {
        if (attacker.Object.HasStateAuthority == false)
        {
            return;
        }

        if (attacker.Runner.Tick < attacker.ActiveStartTick || attacker.Runner.Tick >= attacker.ActiveEndTick)
        {
            return;
        }

        Collect(attacker, profile, Targets);
        for (int i = 0; i < Targets.Count; i++)
        {
            PlayerAvatar victim = Targets[i];
            if (victim.Action == ActionState.Dead || victim.Health <= 0f)
            {
                continue;
            }

            if (HitRegistry.AlreadyLanded(victim, attacker))
            {
                continue;
            }

            DamageResult result = DamageResolver.Resolve(attacker, victim, profile);
            HitRegistry.Mark(victim, attacker, result);
            if (result.Parried == false)
            {
                attacker.AddGauge(profile.GaugeGain);
            }
        }
    }

    static void Collect(PlayerAvatar attacker, AttackProfile profile, List<PlayerAvatar> results)
    {
        results.Clear();
        for (int i = 0; i < PlayerAvatar.All.Count; i++)
        {
            TryAdd(attacker, PlayerAvatar.All[i], profile, results);
        }
    }

    static void TryAdd(PlayerAvatar attacker, PlayerAvatar victim, AttackProfile profile, List<PlayerAvatar> results)
    {
        if (victim == null || victim == attacker || results.Contains(victim))
        {
            return;
        }

        float dx = victim.transform.position.x - attacker.transform.position.x;
        if (Mathf.Abs(dx) > profile.Range + 0.85f)
        {
            return;
        }

        if (Mathf.Abs(victim.transform.position.z - attacker.transform.position.z) > 1.2f)
        {
            return;
        }

        if (attacker.Facing > 0 && dx < -0.15f)
        {
            return;
        }

        if (attacker.Facing < 0 && dx > 0.15f)
        {
            return;
        }

        results.Add(victim);
    }
}
