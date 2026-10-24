// 상태별 허용표. 이동과 전투가 서로 다른 조건을 흩뿌리지 않게 여기만 본다.
public static class ActionRules
{
    public static bool AllowMove(ActionState action)
    {
        return action == ActionState.None
            || action == ActionState.Guard
            || action == ActionState.Attack;
    }

    public static float MoveScale(ActionState action)
    {
        if (action == ActionState.Guard)
        {
            return GameConfig.GuardMoveScale;
        }

        if (action == ActionState.Attack || action == ActionState.Counter || action == ActionState.Ultimate)
        {
            return 0f;
        }

        if (action == ActionState.None)
        {
            return 1f;
        }

        return 0f;
    }

    public static bool AllowJump(ActionState action)
    {
        return action == ActionState.None;
    }

    public static bool AllowGuard(ActionState action)
    {
        return action == ActionState.None || action == ActionState.Guard;
    }

    public static bool AllowNewAttack(ActionState action)
    {
        return action == ActionState.None;
    }

    public static bool AllowDash(ActionState action)
    {
        return action == ActionState.None;
    }

    public static bool AllowUltimate(ActionState action)
    {
        return action == ActionState.None;
    }

    public static bool AllowCounter(ActionState action)
    {
        return action == ActionState.None || action == ActionState.Guard;
    }
}
