using Fusion;
using UnityEngine;

// X축 이동만 허용한다. 바라보는 방향은 상대 위치이고, 대시는 그 방향의 전진 또는 후퇴다.
public class PlayerMotor
{
    public void Tick(PlayerAvatar avatar, CombatNetworkInput input)
    {
        NetworkCharacterController character = avatar.Character;
        if (character == null)
        {
            return;
        }

        FaceOpponent(avatar);
        bool locked = avatar.Playing == false || avatar.Action == ActionState.Dead || avatar.Action == ActionState.HitStun
            || avatar.Action == ActionState.Counter || avatar.Action == ActionState.Ultimate || avatar.Action == ActionState.Dash;

        if (avatar.Action == ActionState.Dash)
        {
            ApplyDash(avatar, character);
            avatar.Movement = MovementState.Dash;
            LockLane(avatar);
            return;
        }

        if (avatar.Action == ActionState.HitStun)
        {
            ApplyKnockback(avatar, character);
            avatar.Movement = MovementState.Knockback;
            LockLane(avatar);
            return;
        }

        if (avatar.Action == ActionState.Dead)
        {
            character.maxSpeed = 0f;
            character.Move(Vector3.zero);
            avatar.Movement = MovementState.Idle;
            avatar.IsCrouching = false;
            LockLane(avatar);
            return;
        }

        bool grounded = character.Grounded;
        bool crouchHeld = input.Held(ActionButton.Crouch);
        bool canCrouch = locked == false && grounded && avatar.Action != ActionState.Attack;
        if (avatar.Action != ActionState.Attack)
        {
            avatar.IsCrouching = canCrouch && crouchHeld;
        }
        ApplyStance(avatar);

        bool wantJump = avatar.BufferFresh(avatar.JumpBufferTick);
        if (locked == false && avatar.IsCrouching == false && wantJump && ActionRules.AllowJump(avatar.Action) && grounded)
        {
            character.Jump();
            avatar.JumpBufferTick = 0;
            avatar.IsCrouching = false;
        }

        int horizontal = locked || avatar.IsCrouching ? 0 : input.Horizontal;
        float scale = locked ? 0f : ActionRules.MoveScale(avatar.Action);
        if (avatar.Playing == false)
        {
            scale = 0f;
        }

        if (horizontal != 0 && horizontal != avatar.Facing)
        {
            scale *= GameConfig.BackWalkScale;
        }

        character.maxSpeed = GameConfig.WalkSpeed * scale;
        character.acceleration = 50f;
        character.braking = 35f;
        character.Move(scale <= 0f || horizontal == 0 ? Vector3.zero : new Vector3(horizontal, 0f, 0f));
        FaceOpponent(avatar);
        LockLane(avatar);

        if (grounded == false && character.Velocity.y > 0.1f)
        {
            avatar.Movement = MovementState.Jump;
        }
        else if (grounded == false)
        {
            avatar.Movement = MovementState.Fall;
        }
        else if (avatar.IsCrouching)
        {
            avatar.Movement = MovementState.Idle;
        }
        else if (horizontal != 0 && scale > 0f)
        {
            avatar.Movement = MovementState.Move;
        }
        else
        {
            avatar.Movement = MovementState.Idle;
        }
    }

    public static void LockLane(PlayerAvatar avatar)
    {
        Vector3 position = avatar.transform.position;
        position.x = Mathf.Clamp(position.x, -GameConfig.StageLimit, GameConfig.StageLimit);
        position.z = GameConfig.LaneZ;
        avatar.transform.position = position;
        Separate(avatar);
        position = avatar.transform.position;
        position.x = Mathf.Clamp(position.x, -GameConfig.StageLimit, GameConfig.StageLimit);
        position.z = GameConfig.LaneZ;
        avatar.transform.position = position;
    }

    static void Separate(PlayerAvatar avatar)
    {
        for (int i = 0; i < PlayerAvatar.All.Count; i++)
        {
            PlayerAvatar other = PlayerAvatar.All[i];
            if (other == null || other == avatar || other.Action == ActionState.Dead)
            {
                continue;
            }

            float gap = avatar.transform.position.x - other.transform.position.x;
            float min = GameConfig.PushboxHalf * 2f;
            float abs = Mathf.Abs(gap);
            if (abs >= min || abs < 0.0001f)
            {
                continue;
            }

            float sign = gap > 0f ? 1f : -1f;
            if (Mathf.Abs(gap) < 0.0001f)
            {
                sign = avatar.SpawnIndex == 0 ? -1f : 1f;
            }

            Vector3 position = avatar.transform.position;
            position.x += sign * (min - abs) * 0.5f;
            avatar.transform.position = position;
        }
    }

    static void ApplyDash(PlayerAvatar avatar, NetworkCharacterController character)
    {
        if (avatar.Runner.Tick >= avatar.DashEndTick)
        {
            avatar.Action = ActionState.None;
            return;
        }

        float speed = avatar.DashDirection.x * avatar.Facing >= 0f ? GameConfig.DashSpeed : GameConfig.BackDashSpeed;
        character.maxSpeed = speed;
        character.acceleration = 400f;
        character.braking = 0f;
        character.Move(new Vector3(Mathf.Sign(avatar.DashDirection.x), 0f, 0f));
        FaceOpponent(avatar);
    }

    static void FaceOpponent(PlayerAvatar avatar)
    {
        PlayerAvatar other = avatar.Opponent();
        int facing = avatar.SpawnIndex == 0 ? 1 : -1;
        if (other != null)
        {
            float dx = other.transform.position.x - avatar.transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                facing = dx > 0f ? 1 : -1;
            }
        }

        avatar.Facing = (sbyte)facing;
        if (avatar.Action != ActionState.Dead)
        {
            avatar.transform.rotation = GameConfig.FacingRotation(facing);
        }
    }

    static void ApplyStance(PlayerAvatar avatar)
    {
        CharacterController body = avatar.GetComponent<CharacterController>();
        if (body == null)
        {
            return;
        }

        if (avatar.IsCrouching)
        {
            body.height = 1.15f;
            body.center = new Vector3(0f, 0.58f, 0f);
        }
        else
        {
            body.height = 2f;
            body.center = new Vector3(0f, 1f, 0f);
        }
    }

    static void ApplyKnockback(PlayerAvatar avatar, NetworkCharacterController character)
    {
        int duration = Mathf.Max(1, avatar.HitStunEndTick - avatar.ActionStartTick);
        int elapsed = avatar.Runner.Tick - avatar.ActionStartTick;
        float remain = Mathf.Clamp01(1f - (elapsed / (float)duration));
        Vector3 direction = avatar.KnockDirection;
        direction.y = 0f;
        direction.z = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = new Vector3(-avatar.Facing, 0f, 0f);
        }

        character.maxSpeed = avatar.KnockStrength * remain;
        character.acceleration = 200f;
        character.braking = 8f;
        character.Move(direction.normalized);
        FaceOpponent(avatar);
    }
}
