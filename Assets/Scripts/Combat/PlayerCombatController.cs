using UnityEngine;

// 우선순위: 사망, 경직, 진행 중 필살기·반격·공격·대시, 그 다음 새 입력.
public class PlayerCombatController
{
    public void Tick(PlayerAvatar avatar, CombatNetworkInput input)
    {
        PlayerHealth.ConsumeIncoming(avatar);
        if (avatar.Playing)
        {
            Remember(avatar, input);
        }

        if (avatar.Action == ActionState.Dead)
        {
            return;
        }

        int tick = avatar.Runner.Tick;
        if (avatar.Action == ActionState.HitStun)
        {
            if (tick >= avatar.HitStunEndTick)
            {
                avatar.Action = ActionState.None;
            }
            else
            {
                return;
            }
        }

        if (avatar.Action == ActionState.Dash)
        {
            if (tick >= avatar.DashEndTick)
            {
                avatar.Action = ActionState.None;
            }
            else
            {
                return;
            }
        }

        if (avatar.Action == ActionState.Attack || avatar.Action == ActionState.Counter || avatar.Action == ActionState.Ultimate)
        {
            TickSwing(avatar, input);
        }

        if (avatar.Action != ActionState.None && avatar.Action != ActionState.Guard)
        {
            return;
        }

        if (avatar.Playing == false)
        {
            if (avatar.Action == ActionState.Guard)
            {
                avatar.IsGuarding = false;
                avatar.Action = ActionState.None;
            }

            return;
        }

        bool grounded = avatar.Character == null || avatar.Character.Grounded;
        if (grounded && input.Held(ActionButton.Crouch) && avatar.Action != ActionState.Dash && avatar.Action != ActionState.HitStun)
        {
            avatar.IsCrouching = true;
        }
        else if (avatar.Action != ActionState.Attack)
        {
            avatar.IsCrouching = false;
        }

        if (grounded && avatar.BufferFresh(avatar.UltimateBufferTick) && UltimateController.CanStart(avatar))
        {
            UltimateController.Begin(avatar);
            return;
        }

        if (grounded && avatar.BufferFresh(avatar.CounterBufferTick) && CounterController.Ready(avatar) && ActionRules.AllowCounter(avatar.Action))
        {
            CounterController.Begin(avatar);
            return;
        }

        if (grounded && avatar.Action == ActionState.None && avatar.BufferFresh(avatar.HeavyBufferTick))
        {
            AttackTimeline.Begin(avatar, AttackId.Heavy, ActionState.Attack, 1);
            return;
        }

        if (avatar.Action == ActionState.None && avatar.BufferFresh(avatar.AttackBufferTick))
        {
            AttackTimeline.Begin(avatar, AttackId.Light1, ActionState.Attack, 1);
            return;
        }

        if (grounded && avatar.Action == ActionState.None && avatar.BufferFresh(avatar.DashBufferTick) && DashController.Ready(avatar))
        {
            DashController.Begin(avatar, input);
            return;
        }

        GuardController.Hold(avatar, input.Held(ActionButton.Guard), grounded);
    }

    static void TickSwing(PlayerAvatar avatar, CombatNetworkInput input)
    {
        int tick = avatar.Runner.Tick;
        AttackProfile profile = CombatBalance.Get(avatar.CurrentAttack);
        HitDetectionService.Resolve(avatar, profile);

        if (avatar.Action == ActionState.Attack
            && avatar.DashBufferTick >= avatar.CancelOpenTick
            && avatar.DashBufferTick < avatar.ActionEndTick
            && DashController.Ready(avatar))
        {
            DashController.Begin(avatar, input);
            return;
        }

        ComboController.QueueIfWindow(avatar);
        if (tick < avatar.ActionEndTick)
        {
            return;
        }

        if (avatar.Action == ActionState.Attack && ComboController.TryAdvance(avatar))
        {
            return;
        }

        avatar.Action = ActionState.None;
        avatar.ClearAttack();
    }

    static void Remember(PlayerAvatar avatar, CombatNetworkInput input)
    {
        int tick = avatar.Runner.Tick;
        if (input.Held(ActionButton.Attack)) avatar.AttackBufferTick = tick;
        if (input.Held(ActionButton.Heavy)) avatar.HeavyBufferTick = tick;
        if (input.Held(ActionButton.Dash)) avatar.DashBufferTick = tick;
        if (input.Held(ActionButton.Jump)) avatar.JumpBufferTick = tick;
        if (input.Held(ActionButton.Counter)) avatar.CounterBufferTick = tick;
        if (input.Held(ActionButton.Ultimate)) avatar.UltimateBufferTick = tick;
    }
}
