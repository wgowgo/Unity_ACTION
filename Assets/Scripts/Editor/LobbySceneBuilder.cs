// TextMeshPro 기능을 불러온다
using TMPro;
// 에디터 전용 기능(MenuItem, AssetDatabase 등)을 불러온다
using UnityEditor;
// 에디터에서 씬을 만들고 저장하는 기능을 불러온다
using UnityEditor.SceneManagement;
// Unity 기본 기능을 불러온다
using UnityEngine;
// EventSystem(UI 클릭을 받는 기능)을 불러온다
using UnityEngine.EventSystems;
// 새 Input System 용 UI 입력 모듈을 불러온다
using UnityEngine.InputSystem.UI;
// Unity UI 기능(Button, Image, LayoutGroup 등)을 불러온다
using UnityEngine.UI;

// ※ 수업 준비용 도구 (Editor 폴더 안에 있어서 게임 빌드에는 포함되지 않음)
// 상단 메뉴 [Tools > Action Game > Build Lobby Scene] 을 누르면
// Lobby 씬과 UI 를 자동으로 만들고, LobbyUI 의 변수에 연결까지 해 준다
public static class LobbySceneBuilder
{
    // 만들어질 씬 파일의 저장 위치
    private const string SCENE_PATH = "Assets/Scenes/Lobby.unity";

    // UI 를 만들 때 사용할 기본 이미지 모음
    private static TMP_DefaultControls.Resources resources;

    // 상단 메뉴에 "Tools/Action Game/Build Lobby Scene" 버튼을 추가한다
    [MenuItem("Tools/Action Game/Build Lobby Scene")]
    // 메뉴 버튼을 누르면 실행되는 함수
    public static void Build()
    {
        // TextMeshPro 기본 리소스가 프로젝트에 있는지 확인한다
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            // 없으면 TMP 리소스 설치 창을 띄운다
            TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
            // 설치 후 다시 실행하라는 안내 창을 띄운다
            EditorUtility.DisplayDialog("TextMeshPro", "'Import TMP Essentials' 를 누른 뒤 메뉴를 다시 실행하세요.", "OK");
            // 함수를 끝낸다
            return;
        }

        // 지금 열린 씬에 저장 안 된 내용이 있으면 저장할지 물어보고, 취소하면
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            // 함수를 끝낸다
            return;
        }

        // 기본 이미지 모음을 새로 만든다
        resources = new TMP_DefaultControls.Resources();
        // 버튼에 쓸 Unity 기본 이미지를 불러온다
        resources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        // 입력칸에 쓸 Unity 기본 이미지를 불러온다
        resources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");

        // 아무것도 없는 빈 씬을 새로 만든다
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라 오브젝트를 만들고 Camera 컴포넌트를 가져온다
        Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        // 메인 카메라라고 태그를 붙인다
        camera.tag = "MainCamera";
        // 배경을 단색으로 칠하도록 설정한다
        camera.clearFlags = CameraClearFlags.SolidColor;
        // 배경색을 어두운 남색으로 정한다
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);

        // EventSystem 오브젝트를 만든다 (이게 없으면 버튼이 클릭되지 않는다)
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        // NetworkManager 오브젝트를 만든다
        new GameObject("NetworkManager", typeof(NetworkManager));

        // Canvas 를 만든다 (모든 UI 는 Canvas 안에 있어야 화면에 보인다)
        GameObject canvas = CreateCanvas();

        // Canvas 에 LobbyUI 스크립트를 붙인다
        LobbyUI lobbyUI = canvas.AddComponent<LobbyUI>();

        // ===== 메뉴 화면 =====
        // 메뉴 패널(배경 박스)을 만든다
        GameObject menuPanel = CreatePanel("MenuPanel", canvas.transform);
        // 게임 제목 글자를 만든다
        CreateText("Title", menuPanel.transform, "ACTION ONLINE", 56);
        // 2인 전용이라는 안내 글자를 만든다
        CreateText("Info", menuPanel.transform, "1 vs 1 (2 Players)", 26);
        // 방 이름 입력칸을 만든다
        TMP_InputField roomNameInput = CreateInputField("RoomNameInput", menuPanel.transform, "Room name");
        // [방 만들기] 버튼을 만든다
        Button createButton = CreateButton("CreateButton", menuPanel.transform, "Create Room (Host)");
        // [방 참가] 버튼을 만든다
        Button joinButton = CreateButton("JoinButton", menuPanel.transform, "Join Room (Client)");

        // ===== 방 화면 =====
        // 방 패널(배경 박스)을 만든다
        GameObject roomPanel = CreatePanel("RoomPanel", canvas.transform);
        // "ROOM" 제목 글자를 만든다
        CreateText("RoomTitle", roomPanel.transform, "ROOM", 48);
        // 방 정보를 보여줄 글자를 만든다
        TMP_Text roomInfoText = CreateText("RoomInfoText", roomPanel.transform, "", 30);
        // 방 정보 글자는 3줄이 들어가야 하니 높이를 160 으로 정한다
        roomInfoText.GetComponent<LayoutElement>().preferredHeight = 160;
        // [방 나가기] 버튼을 만든다
        Button leaveButton = CreateButton("LeaveButton", roomPanel.transform, "Leave Room");
        // 방 화면은 처음에 꺼 둔다
        roomPanel.SetActive(false);

        // ===== 안내 메시지 (화면 아래쪽) =====
        // 안내 메시지 글자를 Canvas 바로 아래에 만든다
        TMP_Text statusText = CreateText("StatusText", canvas.transform, "", 26);
        // 글자 색을 주황색으로 정한다
        statusText.color = new Color(1f, 0.8f, 0.4f);
        // 위치/크기를 조절하기 위해 RectTransform 을 가져온다
        RectTransform statusRect = statusText.rectTransform;
        // 기준점 왼쪽을 화면 왼쪽 아래로 정한다
        statusRect.anchorMin = new Vector2(0, 0);
        // 기준점 오른쪽을 화면 오른쪽 아래로 정한다 (가로로 꽉 차게)
        statusRect.anchorMax = new Vector2(1, 0);
        // 글자 상자의 중심점을 아래 가운데로 정한다
        statusRect.pivot = new Vector2(0.5f, 0);
        // 화면 바닥에서 40 만큼 위로 올린다
        statusRect.anchoredPosition = new Vector2(0, 40);
        // 높이를 50 으로 정한다
        statusRect.sizeDelta = new Vector2(0, 50);

        // ===== LobbyUI 변수 연결 (Inspector 에서 드래그하는 것과 같음) =====
        // 메뉴 패널을 연결한다
        lobbyUI.menuPanel = menuPanel;
        // 방 패널을 연결한다
        lobbyUI.roomPanel = roomPanel;
        // 방 이름 입력칸을 연결한다
        lobbyUI.roomNameInput = roomNameInput;
        // [방 만들기] 버튼을 연결한다
        lobbyUI.createButton = createButton;
        // [방 참가] 버튼을 연결한다
        lobbyUI.joinButton = joinButton;
        // 방 정보 글자를 연결한다
        lobbyUI.roomInfoText = roomInfoText;
        // [방 나가기] 버튼을 연결한다
        lobbyUI.leaveButton = leaveButton;
        // 안내 메시지 글자를 연결한다
        lobbyUI.statusText = statusText;

        // 만든 씬을 파일로 저장한다
        EditorSceneManager.SaveScene(scene, SCENE_PATH);

        // Build Settings 씬 목록을 부트스트랩 → 스타트 → 로비 → 로딩 → 플레이 순서로 맞춘다
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            // 게임 실행 시 첫 씬은 Bootstrap 이다
            new EditorBuildSettingsScene("Assets/Scenes/Bootstrap.unity", true),
            // 타이틀 씬
            new EditorBuildSettingsScene("Assets/Scenes/Start.unity", true),
            // 로비 씬
            new EditorBuildSettingsScene(SCENE_PATH, true),
            // 슬라이더 게이지 로딩 씬
            new EditorBuildSettingsScene("Assets/Scenes/Loading.unity", true),
            // 게임 스타트 씬
            new EditorBuildSettingsScene("Assets/Scenes/Play.unity", true),
        };

        // 완료 메시지를 Console 창에 출력한다
        Debug.Log("Lobby 씬 생성 완료 : " + SCENE_PATH);
    }

    // Canvas 를 만드는 함수
    private static GameObject CreateCanvas()
    {
        // Canvas, CanvasScaler, GraphicRaycaster 컴포넌트를 가진 오브젝트를 만든다
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        // 화면 위에 UI 를 바로 그리는 방식으로 설정한다
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        // 화면 크기 조절을 담당하는 CanvasScaler 를 가져온다
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        // 화면 크기가 달라져도 UI 비율이 유지되도록 설정한다
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // 기준 해상도를 1920x1080 으로 정한다
        scaler.referenceResolution = new Vector2(1920, 1080);
        // 가로/세로 중 어느 쪽에 맞출지 정한다 (0.5 = 반반)
        scaler.matchWidthOrHeight = 0.5f;

        // 만든 Canvas 오브젝트를 돌려준다
        return go;
    }

    // 화면 가운데에 패널(배경 박스)을 만드는 함수. 자식들은 위에서 아래로 자동 정렬된다
    private static GameObject CreatePanel(string name, Transform parent)
    {
        // 이미지, 세로 정렬, 크기 자동 조절 컴포넌트를 가진 오브젝트를 만든다
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        // 부모(Canvas) 아래로 넣는다
        go.transform.SetParent(parent, false);

        // 패널 배경색을 정한다
        go.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.21f, 0.95f);

        // 패널 가로 크기를 640 으로 정한다 (세로는 자동)
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 0);

        // 세로 정렬 컴포넌트를 가져온다
        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        // 패널 안쪽 여백을 상하좌우 40 으로 정한다
        layout.padding = new RectOffset(40, 40, 40, 40);
        // 자식들 사이 간격을 16 으로 정한다
        layout.spacing = 16;
        // 자식들의 가로 크기를 패널 너비에 맞춘다
        layout.childControlWidth = true;
        // 자식들의 세로 크기를 각자의 LayoutElement 값으로 정한다
        layout.childControlHeight = true;
        // 자식들을 세로로 억지로 늘리지 않는다
        layout.childForceExpandHeight = false;

        // 자식 개수에 맞춰 패널 높이가 자동으로 늘어나게 한다
        go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 만든 패널을 돌려준다
        return go;
    }

    // 글자를 만드는 함수
    private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize)
    {
        // TextMeshPro 글자 오브젝트를 만든다
        GameObject go = TMP_DefaultControls.CreateText(resources);
        // 오브젝트 이름을 정한다
        go.name = name;
        // 부모 아래로 넣는다
        go.transform.SetParent(parent, false);
        // 패널 안에서 높이를 조절할 수 있게 LayoutElement 를 붙인다
        go.AddComponent<LayoutElement>();

        // 글자 컴포넌트를 가져온다
        TMP_Text label = go.GetComponent<TMP_Text>();
        // 표시할 내용을 넣는다
        label.text = text;
        // 글자 크기를 정한다
        label.fontSize = fontSize;
        // 가운데 정렬로 정한다
        label.alignment = TextAlignmentOptions.Center;
        // 글자 색을 흰색으로 정한다
        label.color = Color.white;
        // 만든 글자를 돌려준다
        return label;
    }

    // 입력칸을 만드는 함수
    private static TMP_InputField CreateInputField(string name, Transform parent, string placeholder)
    {
        // TextMeshPro 입력칸 오브젝트를 만든다
        GameObject go = TMP_DefaultControls.CreateInputField(resources);
        // 오브젝트 이름을 정한다
        go.name = name;
        // 부모 아래로 넣는다
        go.transform.SetParent(parent, false);
        // 입력칸 높이를 60 으로 정한다
        go.AddComponent<LayoutElement>().preferredHeight = 60;

        // 입력칸 컴포넌트를 가져온다
        TMP_InputField input = go.GetComponent<TMP_InputField>();
        // 입력 글자 크기를 26 으로 정한다
        input.pointSize = 26;
        // 비어 있을 때 보이는 안내 글자를 넣는다
        input.placeholder.GetComponent<TMP_Text>().text = placeholder;
        // 만든 입력칸을 돌려준다
        return input;
    }

    // 버튼을 만드는 함수
    private static Button CreateButton(string name, Transform parent, string text)
    {
        // TextMeshPro 버튼 오브젝트를 만든다
        GameObject go = TMP_DefaultControls.CreateButton(resources);
        // 오브젝트 이름을 정한다
        go.name = name;
        // 부모 아래로 넣는다
        go.transform.SetParent(parent, false);
        // 버튼 높이를 64 로 정한다
        go.AddComponent<LayoutElement>().preferredHeight = 64;

        // 버튼 안의 글자 컴포넌트를 가져온다
        TMP_Text label = go.GetComponentInChildren<TMP_Text>();
        // 버튼에 표시할 글자를 넣는다
        label.text = text;
        // 글자 크기를 28 로 정한다
        label.fontSize = 28;
        // 글자 색을 진한 회색으로 정한다
        label.color = new Color(0.1f, 0.1f, 0.1f);
        // 버튼 컴포넌트를 돌려준다
        return go.GetComponent<Button>();
    }
}
