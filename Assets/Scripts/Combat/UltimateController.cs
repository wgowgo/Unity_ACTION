using UnityEngine;

public static class UltimateController
{
    public static bool CanStart(PlayerAvatar avatar)
    {
        return avatar.Gauge + 0.01f >= CombatBalance.Active.UltimateCost
            && ActionRules.AllowUltimate(avatar.Action);
    }

    public static void Begin(PlayerAvatar avatar)
    {
        avatar.Gauge = Mathf.Max(0f, avatar.Gauge - CombatBalance.Active.UltimateCost);
        avatar.UltimateBufferTick = 0;
        AttackTimeline.Begin(avatar, AttackId.Ultimate, ActionState.Ultimate, 0);
    }
}
