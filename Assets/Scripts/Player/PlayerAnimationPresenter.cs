using UnityEngine;

// 네트워크에 올라간 액션 틱으로 몸과 무기를 보여 준다. 판정은 Animator 를 보지 않는다.
public class PlayerAnimationPresenter
{
    static readonly Color[] Colors =
    {
        new Color(0.95f, 0.45f, 0.15f),
        new Color(0.25f, 0.55f, 0.95f),
        new Color(0.25f, 0.75f, 0.4f),
        new Color(0.72f, 0.35f, 0.9f)
    };

    Material bodyMaterial;
    float flashUntil;
    float previousHealth = -1f;

    public void OnSpawned(PlayerAvatar avatar)
    {
        if (avatar.Body == null)
        {
            return;
        }

        Renderer renderer = avatar.Body.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader != null)
        {
            bodyMaterial = new Material(shader);
            renderer.material = bodyMaterial;
        }
        else
        {
            bodyMaterial = renderer.material;
        }

        int index = Mathf.Abs(avatar.SpawnIndex) % Colors.Length;
        bodyMaterial.color = Colors[index];
        previousHealth = avatar.MaxHealth > 0f ? avatar.MaxHealth : avatar.Health;
    }

    public void OnDespawned(PlayerAvatar avatar)
    {
        if (bodyMaterial != null)
        {
            Object.Destroy(bodyMaterial);
            bodyMaterial = null;
        }
    }

    public void Render(PlayerAvatar avatar)
    {
        if (avatar.Body == null)
        {
            return;
        }

        if (previousHealth >= 0f && avatar.Health < previousHealth - 0.01f)
        {
            flashUntil = Time.time + 0.12f;
        }

        previousHealth = avatar.Health;

        Color baseColor = Colors[Mathf.Abs(avatar.SpawnIndex) % Colors.Length];
        if (bodyMaterial != null)
        {
            bodyMaterial.color = Time.time < flashUntil ? Color.white : baseColor;
        }

        if (avatar.Action == ActionState.Dead)
        {
            avatar.Body.localRotation = Quaternion.Euler(80f, 0f, 0f);
            avatar.Body.localPosition = new Vector3(0f, 0.45f, 0f);
            PoseDummy(avatar);
            return;
        }

        avatar.Body.localRotation = Quaternion.identity;
        avatar.Body.localPosition = new Vector3(0f, 1f, 0f);

        if (avatar.Weapon == null || avatar.Runner == null)
        {
            return;
        }

        float swing = 0f;
        if (avatar.Action == ActionState.Attack || avatar.Action == ActionState.Counter || avatar.Action == ActionState.Ultimate)
        {
            int length = Mathf.Max(1, avatar.ActionEndTick - avatar.ActionStartTick);
            float t = Mathf.Clamp01((avatar.Runner.Tick - avatar.ActionStartTick) / (float)length);
            swing = Mathf.Sin(t * Mathf.PI) * (avatar.Action == ActionState.Ultimate ? -110f : -75f);
        }
        else if (avatar.Action == ActionState.Guard)
        {
            swing = -20f;
        }

        avatar.Weapon.localRotation = Quaternion.Euler(swing, 0f, 0f);
        PoseDummy(avatar);
    }

    static void PoseDummy(PlayerAvatar avatar)
    {
        if (avatar.Facing != 0 && avatar.Action != ActionState.Dead)
        {
            avatar.transform.rotation = GameConfig.FacingRotation(avatar.Facing);
        }

        Transform dummy = avatar.transform.Find("FighterDummy");
        if (dummy == null)
        {
            return;
        }

        bool crouch = avatar.IsCrouching && avatar.Action != ActionState.Dead;
        dummy.localScale = new Vector3(1f, crouch ? 0.62f : 1f, 1f);
        dummy.localPosition = crouch ? new Vector3(0f, -0.08f, 0f) : Vector3.zero;
        if (avatar.Action == ActionState.Dead)
        {
            dummy.localRotation = Quaternion.Euler(80f, 0f, 0f);
            dummy.localPosition = new Vector3(0f, 0.35f, 0f);
        }
        else
        {
            dummy.localRotation = Quaternion.identity;
        }
    }
}
