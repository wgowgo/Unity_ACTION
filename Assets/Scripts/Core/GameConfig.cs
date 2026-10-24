using UnityEngine;

// 1대1 2.5D 대전. 좌측이 P1, 우측이 P2다. 에디터 혼자 확인은 MinPlayersToStart 1을 유지한다.
public static class GameConfig
{
    public const int MaxPlayers = NetworkManager.MAX_PLAYERS;
    public const int MinPlayersToStart = 1;
    public const int RoundsToWin = 2;

    public const float MaxHealth = 100f;
    public const float UltimateMax = 100f;
    public const float RoundSeconds = 99f;
    public const float MatchSeconds = RoundSeconds;
    public const float CountdownSeconds = 3f;
    public const float FinishDelaySeconds = 1.6f;
    public const float CounterWindowSeconds = 0.8f;
    public const float InputBufferSeconds = 0.28f;

    public const float WalkSpeed = 4.2f;
    public const float BackWalkScale = 0.72f;
    public const float GuardMoveScale = 0.45f;
    public const float AttackMoveScale = 0.2f;
    public const float DashSpeed = 11f;
    public const float BackDashSpeed = 9f;
    public const float StageLimit = 8.2f;
    public const float LaneZ = 0f;
    public const float PushboxHalf = 0.48f;
    public const float LaunchVelocity = 7.2f;

    public static Vector3 SpawnPoint(int index)
    {
        float x = index == 0 ? -3.1f : 3.1f;
        return new Vector3(x, 0.05f, LaneZ);
    }

    public static Quaternion SpawnRotation(int index)
    {
        return FacingRotation(index == 0 ? 1 : -1);
    }

    public static Quaternion FacingRotation(int facing)
    {
        return Quaternion.Euler(0f, facing >= 0 ? 90f : -90f, 0f);
    }
}
