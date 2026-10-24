using TMPro;
using UnityEngine;

// Play 씬이 열리면 제목 화면을 치우고, 아레나와 전투 HUD 를 붙인다.
public class PlayStartUI : MonoBehaviour
{
    public TMP_Text titleText;

    void Start()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.45f, 0.62f, 0.78f, 1f);
            camera.transform.position = new Vector3(0f, 2.5f, -10.5f);
            camera.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        Transform logo = transform.Find("Logo");
        if (logo != null)
        {
            logo.gameObject.SetActive(false);
        }

        ArenaGeometry.Ensure();

        if (GetComponent<CombatHud>() == null)
        {
            gameObject.AddComponent<CombatHud>();
        }
    }
}
