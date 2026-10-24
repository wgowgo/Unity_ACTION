// Fusion 기능(StartGameResult, ShutdownReason, NetworkRunner)을 불러온다
using Fusion;
// TextMeshPro 기능(TMP_Text, TMP_InputField)을 불러온다
using TMPro;
// Unity 기본 기능(MonoBehaviour, GameObject 등)을 불러온다
using UnityEngine;
// Unity UI 기능(Button)을 불러온다
using UnityEngine.UI;

// 로비 화면을 담당하는 클래스
// - MenuPanel : 방 이름 입력 + [방 만들기] [방 참가] 버튼
// - RoomPanel : 방 정보 표시 + [게임 시작] [방 나가기] 버튼
public class LobbyUI : MonoBehaviour
{
    // Inspector 에 "패널" 이라는 제목을 표시한다
    [Header("패널")]
    // 방 만들기/참가 화면 오브젝트
    public GameObject menuPanel;
    // 방 안에 들어갔을 때 화면 오브젝트
    public GameObject roomPanel;

    // Inspector 에 "메뉴 화면" 이라는 제목을 표시한다
    [Header("메뉴 화면")]
    // 방 이름 입력칸
    public TMP_InputField roomNameInput;
    // [방 만들기] 버튼
    public Button createButton;
    // [방 참가] 버튼
    public Button joinButton;

    // Inspector 에 "방 화면" 이라는 제목을 표시한다
    [Header("방 화면")]
    // 방 이름, 인원 수 등을 보여줄 글자
    public TMP_Text roomInfoText;
    // [방 나가기] 버튼
    public Button leaveButton;
    // [게임 시작] 버튼. 방장이 누르면 로딩 게이지를 거쳐 게임 스타트 씬으로 간다
    public Button startGameButton;

    // [준비] 버튼. 참가자는 준비해야 방장이 시작할 수 있다. 방장은 입장 시 자동으로 준비된다
    public Button readyButton;

    // Inspector 에 "공통" 이라는 제목을 표시한다
    [Header("공통")]
    // "접속 중..." 같은 안내 메시지를 보여줄 글자
    public TMP_Text statusText;

    // 게임 시작을 이미 눌렀는지 표시한다. 버튼을 여러 번 누르는 것을 막는다
    private bool startingGame;

    // 게임이 시작될 때 한 번 호출되는 함수
    private void Start()
    {
        // [방 만들기] 버튼을 누르면 OnClickCreate 함수가 실행되도록 연결한다
        createButton.onClick.AddListener(OnClickCreate);
        // [방 참가] 버튼을 누르면 OnClickJoin 함수가 실행되도록 연결한다
        joinButton.onClick.AddListener(OnClickJoin);
        // [방 나가기] 버튼을 누르면 OnClickLeave 함수가 실행되도록 연결한다
        leaveButton.onClick.AddListener(OnClickLeave);

        // 씬에 [게임 시작] 버튼이 없으면 방 화면 안에 새로 만든다
        if (startGameButton == null)
        {
            // 방 패널의 맨 아래에 버튼을 만든다
            startGameButton = CreateStartGameButton();
        }

        // [게임 시작] 버튼을 누르면 OnClickStartGame 이 실행되도록 연결한다
        startGameButton.onClick.AddListener(OnClickStartGame);

        if (readyButton == null)
        {
            readyButton = CreateReadyButton();
        }

        readyButton.onClick.AddListener(OnClickReady);

        if (string.IsNullOrEmpty(SessionFlow.Notice) == false)
        {
            statusText.text = SessionFlow.Notice;
            SessionFlow.Notice = null;
        }

        // 처음에는 메뉴 화면을 보여준다
        ShowMenuPanel();
    }

    // 매 프레임마다 호출되는 함수
    private void Update()
    {
        // 방 화면이 꺼져 있는지 확인한다
        if (roomPanel.activeSelf == false)
        {
            // 꺼져 있으면 할 일이 없으니 함수를 끝낸다
            return;
        }

        // 방 화면인데 연결이 끊겼는지 확인한다 (예: 방장이 나감)
        if (NetworkManager.Instance.IsInRoom() == false)
        {
            // 메뉴 화면으로 돌아간다
            ShowMenuPanel();
            // 연결이 끊겼다고 안내한다
            statusText.text = "Disconnected.";
            // 아래 코드는 실행하지 않고 함수를 끝낸다
            return;
        }

        // 방 정보를 화면에 갱신한다
        UpdateRoomInfo();

        // [게임 시작] 버튼을 방장/인원 상태에 맞게 갱신한다
        RefreshStartGameButton();
    }

    // [방 만들기] 버튼을 눌렀을 때 실행되는 함수 (async = 안에서 await 를 쓰겠다는 뜻)
    private async void OnClickCreate()
    {
        // 입력칸에 적힌 방 이름을 가져온다 (Trim = 앞뒤 공백 제거)
        string roomName = roomNameInput.text.Trim();

        // 방 이름이 비어 있는지 확인한다
        if (roomName == "")
        {
            // 방 이름을 입력하라고 안내한다
            statusText.text = "Enter a room name.";
            // 함수를 끝낸다
            return;
        }

        // 접속하는 동안 버튼을 못 누르게 막는다 (여러 번 눌리는 것 방지)
        SetButtonsInteractable(false);
        // 방을 만드는 중이라고 안내한다
        statusText.text = "Creating room...";

        // NetworkManager 에게 방을 만들어 달라고 요청하고, 끝날 때까지 기다린 뒤 결과를 저장한다
        StartGameResult result = await NetworkManager.Instance.CreateRoom(roomName);

        // 버튼을 다시 누를 수 있게 한다
        SetButtonsInteractable(true);

        // 성공/실패 결과를 처리한다
        HandleResult(result);
    }

    // [방 참가] 버튼을 눌렀을 때 실행되는 함수
    private async void OnClickJoin()
    {
        // 입력칸에 적힌 방 이름을 가져온다 (Trim = 앞뒤 공백 제거)
        string roomName = roomNameInput.text.Trim();

        // 방 이름이 비어 있는지 확인한다
        if (roomName == "")
        {
            // 방 이름을 입력하라고 안내한다
            statusText.text = "Enter a room name.";
            // 함수를 끝낸다
            return;
        }

        // 접속하는 동안 버튼을 못 누르게 막는다
        SetButtonsInteractable(false);
        // 방에 들어가는 중이라고 안내한다
        statusText.text = "Joining room...";

        // NetworkManager 에게 방에 들어가 달라고 요청하고, 끝날 때까지 기다린 뒤 결과를 저장한다
        StartGameResult result = await NetworkManager.Instance.JoinRoom(roomName);

        // 버튼을 다시 누를 수 있게 한다
        SetButtonsInteractable(true);

        // 성공/실패 결과를 처리한다
        HandleResult(result);
    }

    // [방 나가기] 버튼을 눌렀을 때 실행되는 함수
    private async void OnClickLeave()
    {
        // 나가는 동안 버튼을 못 누르게 막는다
        SetButtonsInteractable(false);

        // 연결을 끊고, 끝날 때까지 기다린다
        await NetworkManager.Instance.LeaveRoom();

        // 버튼을 다시 누를 수 있게 한다
        SetButtonsInteractable(true);

        // 시작 버튼을 다시 누를 수 있는 상태로 되돌린다
        startingGame = false;

        // 메뉴 화면으로 돌아간다
        ShowMenuPanel();
        // 방을 나갔다고 안내한다
        statusText.text = "Left the room.";
    }

    // 방 만들기 / 참가 결과를 처리하는 함수
    private void HandleResult(StartGameResult result)
    {
        // 접속에 성공했는지 확인한다
        if (result.Ok)
        {
            // 성공했으면 방 화면으로 이동한다
            ShowRoomPanel();
            // 함수를 끝낸다
            return;
        }

        // 실패 이유가 "방을 찾을 수 없음" 인지 확인한다
        if (result.ShutdownReason == ShutdownReason.GameNotFound)
        {
            // 그런 이름의 방이 없다고 안내한다
            statusText.text = "Room not found.";
        }
        // 실패 이유가 "방이 꽉 참" 인지 확인한다
        else if (result.ShutdownReason == ShutdownReason.GameIsFull)
        {
            // 이미 2명이 있다고 안내한다
            statusText.text = "Room is full. (" + NetworkManager.MAX_PLAYERS + "/" + NetworkManager.MAX_PLAYERS + ")";
        }
        // 실패 이유가 "같은 이름의 방이 이미 있음" 인지 확인한다
        else if (result.ShutdownReason == ShutdownReason.ServerInRoom
              || result.ShutdownReason == ShutdownReason.GameIdAlreadyExists)
        {
            // 이미 사용 중인 방 이름이라고 안내한다
            statusText.text = "Room name already in use.";
        }
        // 그 밖의 다른 이유로 실패한 경우
        else
        {
            // 실패 이유의 이름을 그대로 보여준다
            statusText.text = "Failed: " + result.ShutdownReason;
        }
    }

    // 방 정보(방 이름, 인원, 내 역할)를 화면에 표시하는 함수
    private void UpdateRoomInfo()
    {
        // NetworkManager 에서 현재 runner 를 가져온다
        NetworkRunner runner = NetworkManager.Instance.runner;

        // 현재 방 이름을 가져온다
        string roomName = runner.SessionInfo.Name;

        // 현재 방에 있는 인원 수를 가져온다
        int playerCount = runner.SessionInfo.PlayerCount;

        // 내 역할(Host / Client)을 담을 변수를 만든다
        string role;
        // 내가 서버(방장)인지 확인한다
        if (runner.IsServer)
        {
            // 방장이면 "Host"
            role = "Host";
        }
        // 서버가 아니면
        else
        {
            // 참가자이므로 "Client"
            role = "Client";
        }

        // 방 이름 / 인원 / 역할을 한 글자로 합쳐서 화면에 표시한다 (\n = 줄바꿈)
        string roster = "";
        if (PlayerSessionDirectory.Instance != null)
        {
            roster = "\n" + PlayerSessionDirectory.Instance.BuildRoster(runner);
        }

        roomInfoText.text = "Room : " + roomName + "\n"
                          + "Players : " + playerCount + " / " + NetworkManager.MAX_PLAYERS + "\n"
                          + "Role : " + role + roster;
    }

    // 메뉴 화면을 보여주는 함수
    private void ShowMenuPanel()
    {
        // 메뉴 화면을 켠다
        menuPanel.SetActive(true);
        // 방 화면을 끈다
        roomPanel.SetActive(false);
        // 안내 메시지를 지운다
        statusText.text = "";
    }

    // 방 화면을 보여주는 함수
    private void ShowRoomPanel()
    {
        // 메뉴 화면을 끈다
        menuPanel.SetActive(false);
        // 방 화면을 켠다
        roomPanel.SetActive(true);
        // 안내 메시지를 지운다
        statusText.text = "";
    }

    // 모든 버튼을 한꺼번에 켜거나 끄는 함수 (value 가 true 면 켜고, false 면 끈다)
    private void SetButtonsInteractable(bool value)
    {
        // [방 만들기] 버튼을 켜거나 끈다
        createButton.interactable = value;
        // [방 참가] 버튼을 켜거나 끈다
        joinButton.interactable = value;
        // [방 나가기] 버튼을 켜거나 끈다
        leaveButton.interactable = value;

        // [게임 시작] 버튼이 있을 때만 같이 켜거나 끈다
        if (startGameButton != null)
        {
            // [게임 시작] 버튼을 켜거나 끈다
            startGameButton.interactable = value;
        }

        if (readyButton != null)
        {
            readyButton.interactable = value;
        }
    }

    // 방 화면 안에 [게임 시작] 버튼을 만드는 함수
    private Button CreateStartGameButton()
    {
        // 버튼 클릭을 받을 EventSystem 이 없으면 만든다
        RuntimeUiFactory.EnsureEventSystem();

        // 방 패널 맨 아래에 버튼을 만든다. 위치는 VerticalLayoutGroup 이 다시 잡는다
        Button button = RuntimeUiFactory.CreateButton(roomPanel.transform, "Start Game", Vector2.zero, new Vector2(560f, 64f));

        // 오브젝트 이름을 정한다
        button.gameObject.name = "StartGameButton";

        // 레이아웃이 높이를 계산할 수 있게 LayoutElement 를 붙인다
        LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();

        // 버튼 높이를 다른 로비 버튼과 같은 64 로 정한다
        layout.preferredHeight = 64f;

        // 버튼 배경을 주황색으로 바꿔 시작 버튼임을 드러낸다
        button.GetComponent<Image>().color = new Color(0.95f, 0.55f, 0.15f, 1f);

        // 만든 버튼을 돌려준다
        return button;
    }

    Button CreateReadyButton()
    {
        RuntimeUiFactory.EnsureEventSystem();
        Button button = RuntimeUiFactory.CreateButton(roomPanel.transform, "Ready", Vector2.zero, new Vector2(560f, 64f));
        button.gameObject.name = "ReadyButton";
        LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 64f;
        button.GetComponent<Image>().color = new Color(0.2f, 0.55f, 0.35f, 1f);
        return button;
    }

    // [게임 시작] 버튼의 글자와 누를 수 있는지를 갱신하는 함수
    private void RefreshStartGameButton()
    {
        // 버튼이 없으면 갱신할 것이 없다
        if (startGameButton == null)
        {
            // 함수를 끝낸다
            return;
        }

        // 버튼 안의 글자 컴포넌트를 가져온다
        TMP_Text label = startGameButton.GetComponentInChildren<TMP_Text>();

        // 이미 시작을 눌렀으면 버튼을 잠그고 안내만 남긴다
        if (startingGame)
        {
            // 버튼을 못 누르게 한다
            startGameButton.interactable = false;

            // 로딩으로 넘어가는 중이라고 표시한다
            label.text = "Starting...";

            // 함수를 끝낸다
            return;
        }

        // 현재 러너를 가져온다
        NetworkRunner runner = NetworkManager.Instance.runner;

        // 내가 방장인지 확인한다. 씬을 바꾸는 권한은 방장에게만 있다
        bool isHost = runner.IsSceneAuthority;

        // 방장이 아니면 손님은 기다린다고 보여 주고 버튼을 끈다
        if (isHost == false)
        {
            // 손님은 누를 수 없다
            startGameButton.interactable = false;

            // 방장을 기다린다는 글자를 넣는다
            label.text = "Waiting for host";

            RefreshReadyButton();

            // 함수를 끝낸다
            return;
        }

        PlayerSessionDirectory directory = PlayerSessionDirectory.Instance;
        bool ready = directory == null || directory.AllReady;
        startGameButton.interactable = ready;

        if (ready)
        {
            label.text = "Start Game";
        }
        else
        {
            label.text = "Waiting for ready";
        }

        RefreshReadyButton();
    }

    void RefreshReadyButton()
    {
        if (readyButton == null)
        {
            return;
        }

        PlayerSessionDirectory directory = PlayerSessionDirectory.Instance;
        bool localReady = directory != null && directory.LocalReady;
        TMP_Text readyLabel = readyButton.GetComponentInChildren<TMP_Text>();
        readyLabel.text = localReady ? "Cancel Ready" : "Ready";
        readyButton.interactable = directory != null && startingGame == false;
    }

    void OnClickReady()
    {
        PlayerSessionDirectory directory = PlayerSessionDirectory.Instance;
        if (directory == null)
        {
            statusText.text = "Waiting for session.";
            return;
        }

        directory.RequestReady(directory.LocalReady == false);
    }

    // [게임 시작] 버튼을 눌렀을 때 실행되는 함수
    private void OnClickStartGame()
    {
        // 이미 시작 중이면 무시한다
        if (startingGame)
        {
            // 함수를 끝낸다
            return;
        }

        PlayerSessionDirectory directory = PlayerSessionDirectory.Instance;
        if (directory != null && directory.AllReady == false)
        {
            statusText.text = "All players must be ready.";
            return;
        }

        // 방에 없거나 씬을 바꿀 권한이 없으면 시작하지 않는다
        if (NetworkManager.Instance.IsInRoom() == false || NetworkManager.Instance.runner.IsSceneAuthority == false)
        {
            // 방장만 게임을 시작할 수 있다고 안내한다
            statusText.text = "Only the host can start.";

            // 함수를 끝낸다
            return;
        }

        // 시작 중이라고 표시한다
        startingGame = true;

        // 다른 버튼도 잠시 못 누르게 한다
        SetButtonsInteractable(false);

        // 로딩 씬으로 넘어간다고 안내한다
        statusText.text = "Loading game...";

        // 로딩 씬(슬라이더 게이지)을 거쳐 게임 스타트 씬(Play)으로 간다
        SceneFlow.LoadThroughGauge(SceneNames.Play);
    }
}
