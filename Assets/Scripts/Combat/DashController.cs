using Fusion;
using UnityEngine;

public static class DashController
{
    public static bool Ready(PlayerAvatar avatar)
    {
        if (avatar.DashCooldown.IsRunning == false)
        {
            return true;
        }

        return avatar.DashCooldown.Expired(avatar.Runner);
    }

    public static void Begin(PlayerAvatar avatar, CombatNetworkInput input)
    {
        int horizontal = input.Horizontal;
        if (horizontal == 0)
        {
            horizontal = avatar.Facing >= 0 ? 1 : -1;
        }

        avatar.DashDirection = new Vector3(horizontal, 0f, 0f);
        avatar.Action = ActionState.Dash;
        avatar.IsGuarding = false;
        avatar.ActionStartTick = avatar.Runner.Tick;
        int length = CombatBalance.Ticks(avatar.Runner, CombatBalance.Active.DashActive + CombatBalance.Active.DashRecovery);
        avatar.DashEndTick = avatar.Runner.Tick + length;
        avatar.DashCooldown = TickTimer.CreateFromSeconds(avatar.Runner, CombatBalance.Active.DashCooldown);
        avatar.DashBufferTick = 0;
        avatar.ClearAttack();
    }
}
