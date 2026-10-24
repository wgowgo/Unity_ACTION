using UnityEngine;

// 선딜, 활성, 후딜을 틱으로 고정한다. 애니메이션 시간이 아니라 이 틱이 판정의 기준이다.
public static class AttackTimeline
{
    public static void Begin(PlayerAvatar avatar, AttackId id, ActionState state, int comboStep)
    {
        AttackProfile profile = CombatBalance.Get(id);
        int start = avatar.Runner.Tick;
        int startup = CombatBalance.Ticks(avatar.Runner, profile.Startup);
        int active = CombatBalance.Ticks(avatar.Runner, profile.Active);
        int recovery = CombatBalance.Ticks(avatar.Runner, profile.Recovery);

        avatar.AttackSerial += 1;
        if (avatar.AttackSerial <= 0)
        {
            avatar.AttackSerial = 1;
        }

        avatar.Action = state;
        avatar.CurrentAttack = id;
        avatar.SwingHeight = FighterRules.HeightFor(avatar, id);
        avatar.ComboStep = comboStep;
        avatar.ComboQueued = false;
        avatar.IsGuarding = false;
        avatar.ActionStartTick = start;
        avatar.ActiveStartTick = start + startup;
        avatar.ActiveEndTick = start + startup + active;
        avatar.ActionEndTick = start + startup + active + recovery;
        avatar.ComboOpenTick = avatar.ActiveEndTick;
        avatar.ComboCloseTick = avatar.ActiveEndTick + Mathf.Max(1, recovery / 2);
        avatar.CancelOpenTick = avatar.ActionEndTick - Mathf.Max(1, recovery / 3);
        avatar.AttackBufferTick = 0;
    }
}
