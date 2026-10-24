using UnityEngine;

public static class GuardController
{
    public static void Hold(PlayerAvatar avatar, bool held, bool grounded)
    {
        if (held && grounded && avatar.Playing && ActionRules.AllowGuard(avatar.Action))
        {
            if (avatar.IsGuarding == false)
            {
                avatar.GuardStartTick = avatar.Runner.Tick;
            }

            avatar.IsGuarding = true;
            avatar.Action = ActionState.Guard;
            return;
        }

        if (avatar.Action == ActionState.Guard)
        {
            avatar.IsGuarding = false;
            avatar.Action = ActionState.None;
        }
    }
}
