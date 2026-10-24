// Unity 기본 기능(MonoBehaviour, Camera, Color 등)을 불러온다
using UnityEngine;
// 버튼을 쓰기 위해 불러온다
using UnityEngine.UI;

// Start 씬(타이틀)을 담당하는 클래스. START 버튼을 누르면 로비로 간다
public class StartUI : MonoBehaviour
{
    // 씬에 미리 배치해 둔 START 버튼
    public Button startButton;

    // 오브젝트가 켜질 때 한 번 호출되는 함수
    private void Start()
    {
        // 메인 카메라를 찾는다
        Camera camera = Camera.main;

        // 카메라가 있으면 배경을 로비와 같은 어두운 색으로 바꾼다
        if (camera != null)
        {
            // 배경을 단색으로 그린다
            camera.clearFlags = CameraClearFlags.SolidColor;

            // 배경색을 어두운 남색으로 정한다
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
        }

        // 씬에 버튼이 이미 연결되어 있으면 그 버튼을 쓴다
        if (startButton != null)
        {
            // 씬의 버튼에 함수가 아직 없으면 코드로 연결한다
            if (startButton.onClick.GetPersistentEventCount() == 0)
            {
                // 버튼을 누르면 OnClickStart 가 실행되게 한다
                startButton.onClick.AddListener(OnClickStart);
            }

            // 이미 배치된 UI 를 다시 만들지 않는다
            return;
        }

        // 이 오브젝트에 화면 전체 캔버스를 붙인다
        RuntimeUiFactory.BuildOverlay(gameObject);

        // 버튼 클릭을 받을 EventSystem 이 없으면 만든다
        RuntimeUiFactory.EnsureEventSystem();

        // 게임 제목 글자를 화면 가운데 위쪽에 만든다
        RuntimeUiFactory.CreateLabel(transform, "ACTION", 92, new Vector2(0f, 160f), new Vector2(1200f, 140f));

        // 안내 글자를 제목 아래에 만든다
        RuntimeUiFactory.CreateLabel(transform, "PRESS START", 32, new Vector2(0f, 40f), new Vector2(1200f, 80f));

        // START 버튼을 안내 글자 아래에 만든다
        startButton = RuntimeUiFactory.CreateButton(transform, "START", new Vector2(0f, -120f), new Vector2(420f, 90f));

        // 버튼을 누르면 OnClickStart 가 실행되도록 연결한다
        startButton.onClick.AddListener(OnClickStart);
    }

    // START 버튼을 눌렀을 때 실행되는 함수
    public void OnClickStart()
    {
        // 연타로 씬이 여러 번 열리는 것을 막는다
        startButton.interactable = false;

        // 로비 씬으로 바로 넘어간다 (로딩 게이지는 로비에서 게임을 시작할 때 쓴다)
        SceneFlow.Load(SceneNames.Lobby);
    }
}
