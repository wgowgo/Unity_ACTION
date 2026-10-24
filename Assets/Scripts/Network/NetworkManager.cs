// async / await 를 쓰기 위해 Task 기능을 불러온다
using System.Threading.Tasks;
// Photon Fusion 2 기능(NetworkRunner, GameMode 등)을 불러온다
using Fusion;
// Unity 기본 기능(MonoBehaviour, GameObject 등)을 불러온다
using UnityEngine;

// 네트워크 연결(방 만들기 / 방 참가 / 방 나가기)을 담당하는 클래스
public class NetworkManager : MonoBehaviour
{
    // 어디서든 NetworkManager.Instance 로 접근할 수 있게 하는 변수 (싱글톤)
    public static NetworkManager Instance;

    // 1대1 대전. 방장과 상대 한 명만 들어온다
    public const int MAX_PLAYERS = 2;

    // Fusion 의 핵심 객체. 네트워크 연결과 동기화를 전부 처리한다
    public NetworkRunner runner;

    // 게임 오브젝트가 생성될 때 가장 먼저 한 번 호출되는 함수
    private void Awake()
    {
        // 이미 NetworkManager 가 존재하는지 확인한다 (씬을 다시 불러온 경우 등)
        if (Instance != null)
        {
            // 이미 있으니 새로 생긴 나 자신은 삭제한다
            Destroy(gameObject);
            // 아래 코드는 실행하지 않고 함수를 끝낸다
            return;
        }

        // 내가 유일한 NetworkManager 라고 등록한다
        Instance = this;

        // 씬이 바뀌어도 이 오브젝트가 사라지지 않게 한다
        DontDestroyOnLoad(gameObject);
    }

    // [방 만들기] 함수. 내가 Host(방장)가 된다. 결과(StartGameResult)를 돌려준다
    public async Task<StartGameResult> CreateRoom(string roomName)
    {
        // Host 모드로 접속하고, 끝날 때까지 기다린 뒤 결과를 돌려준다
        return await StartGame(GameMode.Host, roomName);
    }

    // [방 참가] 함수. 나는 Client(참가자)가 된다. 결과(StartGameResult)를 돌려준다
    public async Task<StartGameResult> JoinRoom(string roomName)
    {
        // Client 모드로 접속하고, 끝날 때까지 기다린 뒤 결과를 돌려준다
        return await StartGame(GameMode.Client, roomName);
    }

    // [방 나가기] 함수
    public async Task LeaveRoom()
    {
        // runner 가 있는지 확인한다 (= 방에 들어가 있는지)
        if (runner != null)
        {
            // 네트워크 연결을 끊고, 끝날 때까지 기다린다
            await runner.Shutdown();
        }

        // 연결이 끊겼으니 runner 변수를 비운다
        runner = null;
    }

    // 지금 방에 들어가 있는지 true / false 로 알려주는 함수
    public bool IsInRoom()
    {
        // runner 가 있고(null 아님) 실행 중이면(IsRunning) true 를 돌려준다
        return runner != null && runner.IsRunning;
    }

    // 방 만들기 / 방 참가의 공통 부분. mode 로 Host 인지 Client 인지 구분한다
    private async Task<StartGameResult> StartGame(GameMode mode, string roomName)
    {
        // 혹시 이전 연결이 남아 있으면 먼저 끊는다
        await LeaveRoom();

        // NetworkRunner 를 붙일 빈 게임 오브젝트를 새로 만든다 (runner 는 한 번 끊기면 재사용 불가)
        GameObject runnerObject = new GameObject("NetworkRunner");

        // 씬이 바뀌어도 runner 오브젝트가 사라지지 않게 한다
        DontDestroyOnLoad(runnerObject);

        // 빈 오브젝트에 NetworkRunner 컴포넌트를 붙이고 변수에 저장한다
        runner = runnerObject.AddComponent<NetworkRunner>();

        // 이 컴퓨터의 키보드/마우스 입력을 네트워크로 보내겠다고 설정한다 (캐릭터 조작 때 필요)
        runner.ProvideInput = true;

        // 입력 수집과 세션 흐름은 StartGame 도중 콜백이 오므로, 접속보다 먼저 등록한다
        FusionInputProvider inputProvider = runnerObject.AddComponent<FusionInputProvider>();
        SessionFlow sessionFlow = runnerObject.AddComponent<SessionFlow>();
        runner.AddCallbacks(inputProvider);
        runner.AddCallbacks(sessionFlow);

        // 방 접속에 필요한 설정값을 담을 상자를 만든다
        StartGameArgs args = new StartGameArgs();

        

        // Host(방장) 인지 Client(참가자) 인지 설정한다
        args.GameMode = mode;

        // 방 이름을 설정한다 (같은 이름을 입력해야 같은 방에 들어간다)
        args.SessionName = roomName;

        // 최대 인원을 2명으로 설정한다
        args.PlayerCount = MAX_PLAYERS;

        // 네트워크 씬 전환을 담당하는 컴포넌트를 붙인다
        // GameSceneManager 는 로딩 진행률을 슬라이더 게이지로 넘긴다
        args.SceneManager = runnerObject.AddComponent<GameSceneManager>();

        // 실제로 서버에 접속하고, 끝날 때까지 기다린 뒤 결과를 result 에 저장한다
        StartGameResult result = await runner.StartGame(args);

        // 접속에 실패했는지 확인한다
        if (result.Ok == false)
        {
            // 실패한 runner 오브젝트를 삭제한다
            Destroy(runnerObject);

            // runner 변수도 비운다
            runner = null;
        }

        // 성공/실패 결과를 호출한 쪽(LobbyUI)에 돌려준다
        return result;
    }
}
