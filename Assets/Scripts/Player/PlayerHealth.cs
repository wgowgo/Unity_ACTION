using Fusion;
using UnityEngine;

// 예약된 피격을 다음 틱 시작에 체력과 경직으로 반영한다. 호스트만 수치를 바꾼다.
public static class PlayerHealth
{
    public static void ConsumeIncoming(PlayerAvatar avatar)
    {
        if (avatar.Object.HasStateAuthority == false)
        {
            return;
        }

        if (avatar.IncomingSerial == 0 || avatar.Action == ActionState.Dead)
        {
            return;
        }

        if (avatar.ConsumedSerial == avatar.IncomingSerial && avatar.ConsumedFrom == avatar.IncomingFrom)
        {
            return;
        }

        avatar.ConsumedSerial = avatar.IncomingSerial;
        avatar.ConsumedFrom = avatar.IncomingFrom;

        if (avatar.IncomingParry || avatar.IncomingGuard)
        {
            avatar.CounterWindow = TickTimer.CreateFromSeconds(avatar.Runner, CombatBalance.Active.CounterWindow);
        }

        if (avatar.IncomingParry)
        {
            avatar.IsGuarding = true;
            return;
        }

        float next = Mathf.Clamp(avatar.Health - avatar.IncomingDamage, 0f, avatar.MaxHealth);
        avatar.Health = next;
        avatar.IsGuarding = false;
        avatar.ComboQueued = false;

        if (next <= 0f)
        {
            avatar.Deaths += 1;
            avatar.Action = ActionState.Dead;
            avatar.ClearAttack();
            PlayerAvatar attacker = PlayerAvatar.Find(avatar.IncomingFrom);
            if (attacker != null && attacker != avatar)
            {
                attacker.Kills += 1;
            }

            return;
        }

        if (avatar.IncomingStunTicks > 0)
        {
            avatar.Action = ActionState.HitStun;
            avatar.ActionStartTick = avatar.Runner.Tick;
            avatar.HitStunEndTick = avatar.Runner.Tick + avatar.IncomingStunTicks;
            avatar.KnockDirection = avatar.IncomingKnock;
            avatar.KnockStrength = avatar.IncomingKnockStrength;
            avatar.ClearAttack();
            if (avatar.IncomingLaunch && avatar.Character != null)
            {
                UnityEngine.Vector3 velocity = avatar.Character.Velocity;
                velocity.y = GameConfig.LaunchVelocity;
                avatar.Character.Velocity = velocity;
            }
        }
    }
}
