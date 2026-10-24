using UnityEngine;

// 약공격 두 번 뒤에 강공격으로 이어진다. 창 안에서 누른 입력만 예약한다.
public static class ComboController
{
    public static void QueueIfWindow(PlayerAvatar avatar)
    {
        if (avatar.Action != ActionState.Attack || avatar.ComboStep >= 3)
        {
            return;
        }

        int press = Mathf.Max(avatar.AttackBufferTick, avatar.HeavyBufferTick);
        if (press < avatar.ComboOpenTick || press > avatar.ComboCloseTick)
        {
            return;
        }

        avatar.ComboQueued = true;
        avatar.AttackBufferTick = 0;
    }

    public static bool TryAdvance(PlayerAvatar avatar)
    {
        if (avatar.ComboQueued == false || avatar.ComboStep >= 3)
        {
            return false;
        }

        AttackId next = avatar.ComboStep <= 1 ? AttackId.Light2 : AttackId.Heavy;
        int step = avatar.ComboStep + 1;
        AttackTimeline.Begin(avatar, next, ActionState.Attack, step);
        return true;
    }
}
