// 자세와 기술 종류로 상단, 중단, 하단을 정한다. 판정 높이는 기술이 시작된 틱에 고정된다.
public static class FighterRules
{
    public static AttackHeight HeightFor(PlayerAvatar avatar, AttackId id)
    {
        if (avatar.Character != null && avatar.Character.Grounded == false)
        {
            return AttackHeight.High;
        }

        if (avatar.IsCrouching)
        {
            return AttackHeight.Low;
        }

        if (id == AttackId.Heavy || id == AttackId.Light3)
        {
            return AttackHeight.Mid;
        }

        return AttackHeight.High;
    }
}
