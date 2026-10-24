using UnityEngine;

// 두 캐릭터를 함께 담는 측면 카메라. P1/P2 HUD 위치는 화면 좌우가 바뀌어도 슬롯을 유지한다.
public class PlayerCamera : MonoBehaviour
{
    public float shake;

    PlayerAvatar avatar;

    void Awake()
    {
        avatar = GetComponent<PlayerAvatar>();
    }

    void LateUpdate()
    {
        if (avatar == null || avatar.Object == null || avatar.Object.IsValid == false || avatar.HasInputAuthority == false)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        float left = avatar.transform.position.x;
        float right = left;
        float shakeTarget = 0f;
        for (int i = 0; i < PlayerAvatar.All.Count; i++)
        {
            PlayerAvatar fighter = PlayerAvatar.All[i];
            if (fighter == null)
            {
                continue;
            }

            left = Mathf.Min(left, fighter.transform.position.x);
            right = Mathf.Max(right, fighter.transform.position.x);
            if (fighter.Action == ActionState.HitStun || fighter.Action == ActionState.Ultimate)
            {
                shakeTarget = 0.16f;
            }
        }

        shake = Mathf.MoveTowards(shake, shakeTarget, Time.deltaTime * (shakeTarget > 0f ? 8f : 1.4f));
        float span = Mathf.Max(3.2f, right - left);
        float distance = Mathf.Clamp(7.4f + span * 0.42f, 8.2f, 13.5f);
        float midX = Mathf.Clamp((left + right) * 0.5f, -GameConfig.StageLimit + 2f, GameConfig.StageLimit - 2f);
        Vector3 look = new Vector3(midX, 1.35f, GameConfig.LaneZ);
        Vector3 desired = look + new Vector3(0f, 1.15f, -distance);
        if (shake > 0.01f)
        {
            desired += new Vector3(Mathf.Sin(Time.time * 70f), Mathf.Cos(Time.time * 63f), 0f) * shake;
        }

        camera.transform.position = desired;
        camera.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }
}
