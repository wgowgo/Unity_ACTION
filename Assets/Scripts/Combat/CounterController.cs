using Fusion;

public static class CounterController
{
    public static bool Ready(PlayerAvatar avatar)
    {
        return avatar.CounterWindow.IsRunning && avatar.CounterWindow.Expired(avatar.Runner) == false;
    }

    public static void Begin(PlayerAvatar avatar)
    {
        avatar.CounterBufferTick = 0;
        avatar.CounterWindow = TickTimer.None;
        AttackTimeline.Begin(avatar, AttackId.Counter, ActionState.Counter, 0);
    }
}
