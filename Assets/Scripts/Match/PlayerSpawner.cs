using Fusion;
using UnityEngine;

// 스폰 위치와 입력 권한은 SessionFlow 가 연결하고, 이 클래스는 인덱스 규칙을 한곳에 둔다.
public static class PlayerSpawner
{
    public static void Place(PlayerAvatar avatar, int index)
    {
        if (avatar == null)
        {
            return;
        }

        Vector3 position = GameConfig.SpawnPoint(index);
        Quaternion rotation = GameConfig.SpawnRotation(index);
        if (avatar.Character != null)
        {
            avatar.Character.Teleport(position, rotation);
            avatar.Character.Velocity = Vector3.zero;
        }
        else
        {
            avatar.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
