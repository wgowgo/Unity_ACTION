// Unity 기본 기능(MonoBehaviour, GameObject 등)을 불러온다
using UnityEngine;

// 게임의 맨 처음 씬에서 한 번만 초기화하고 Start 씬으로 보내는 클래스
public class GameBootstrap : MonoBehaviour
{
    // 어디서든 이 초기화 객체를 가리키는 변수 (싱글톤)
    public static GameBootstrap Instance;

    // 오브젝트가 생길 때 가장 먼저 한 번 호출되는 함수
    private void Awake()
    {
        // 이미 부트스트랩이 살아 있는지 확인한다 (씬이 다시 열린 경우)
        if (Instance != null)
        {
            // 새로 생긴 나 자신은 지운다
            Destroy(gameObject);

            // 아래 초기화는 하지 않고 함수를 끝낸다
            return;
        }

        // 내가 유일한 부트스트랩이라고 등록한다
        Instance = this;

        // 씬이 바뀌어도 이 오브젝트가 사라지지 않게 한다
        DontDestroyOnLoad(gameObject);

        // 네트워크 매니저가 아직 없으면 여기서 만들어 둔다
        if (NetworkManager.Instance == null)
        {
            // 빈 오브젝트를 만들고 NetworkManager 를 붙인다 (붙이자마자 그 스크립트의 Awake 가 실행된다)
            new GameObject("NetworkManager").AddComponent<NetworkManager>();
        }
    }

    // 첫 프레임이 시작될 때 한 번 호출되는 함수
    private void Start()
    {
        // 초기화가 끝났으니 타이틀(Start) 씬으로 넘어간다
        SceneFlow.Load(SceneNames.Start);
    }
}
