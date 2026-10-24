// 코루틴(IEnumerator)을 쓰기 위해 불러온다
using System.Collections;
// Fusion 러너가 씬을 바꾸는 중인지 확인하기 위해 불러온다
using Fusion;
// TextMeshPro 글자를 쓰기 위해 불러온다
using TMPro;
// Unity 기본 기능(MonoBehaviour, Mathf, Time 등)을 불러온다
using UnityEngine;
// 씬 로드 진행률(AsyncOperation)과 씬 이름을 쓰기 위해 불러온다
using UnityEngine.SceneManagement;
// 슬라이더를 게이지로 쓰기 위해 불러온다
using UnityEngine.UI;

// Loading 씬을 담당하는 클래스
// 슬라이더의 value(0~1)를 게이지바로 보여주고, 가득 차면 게임 스타트 씬(Play)으로 넘어간다
public class LoadingUI : MonoBehaviour
{
    // 게이지가 0 에서 1 까지 차는 데 쓸 최소 시간(초). 씬이 너무 빨리 열려도 막대가 보이게 한다
    private const float FILL_SECONDS = 1.1f;

    // 씬에 미리 배치해 둔 게이지 슬라이더
    public Slider gauge;

    // 씬에 미리 배치해 둔 퍼센트 글자
    public TMP_Text percentText;

    // 로딩이 끝난 뒤 열 씬 이름
    private string nextSceneName;

    // 방에 들어간 상태에서 Fusion 이 씬을 바꾸는지 표시한다
    private bool useFusion;

    // 슬라이더가 따라갈 목표 진행률 (0~1)
    private float target;

    // 화면에 실제로 그려 줄 진행률 (목표를 향해 천천히 따라간다)
    private float shown;

    // 방에 들어가지 않았을 때 Unity 가 씬을 읽는 작업
    private AsyncOperation loadOperation;

    // 이미 화면을 지웠는지 표시한다. 두 번 지우는 것을 막는다
    private bool closing;

    // 오브젝트가 생길 때 한 번 호출되는 함수
    private void Start()
    {
        // 메인 카메라를 찾는다
        Camera camera = Camera.main;

        // 카메라가 있으면 로딩 화면 배경을 어둡게 한다
        if (camera != null)
        {
            // 배경을 단색으로 그린다
            camera.clearFlags = CameraClearFlags.SolidColor;

            // 배경색을 거의 검정에 가까운 남색으로 정한다
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f, 1f);
        }

        // 씬에 슬라이더와 글자가 없으면 실행 중에 만든다
        if (gauge == null || percentText == null)
        {
            // 이 오브젝트에 화면 전체 캔버스를 붙인다
            RuntimeUiFactory.BuildOverlay(gameObject);

            // "LOADING" 제목을 게이지 위에 만든다
            RuntimeUiFactory.CreateLabel(transform, "LOADING", 72, new Vector2(0f, 140f), new Vector2(1200f, 120f));

            // 슬라이더를 화면 가운데에 만들고, 이 슬라이더가 게이지바다
            gauge = RuntimeUiFactory.CreateGauge(transform, new Vector2(0f, 0f), new Vector2(900f, 48f));

            // 퍼센트 글자를 게이지 아래에 만든다
            percentText = RuntimeUiFactory.CreateLabel(transform, "0%", 36, new Vector2(0f, -80f), new Vector2(400f, 80f));
        }

        // 로비에서 저장해 둔 다음 씬 이름을 가져온다
        nextSceneName = SceneFlow.NextSceneName;

        // 이름이 비어 있으면 게임 스타트 씬(Play)을 연다
        if (string.IsNullOrEmpty(nextSceneName))
        {
            // 기본 목적지를 Play 로 정한다
            nextSceneName = SceneNames.Play;
        }

        // 네트워크 매니저를 가져온다
        NetworkManager manager = NetworkManager.Instance;

        // 방에 들어가 있으면 Fusion 이 씬을 함께 바꿔야 한다
        useFusion = manager != null && manager.IsInRoom();

        // 씬이 실제로 열렸는지 알림을 받는다
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Fusion 경로이면 진행률 이벤트도 구독한다
        if (useFusion)
        {
            // Play 씬이 열리는 동안에도 게이지가 남아 있도록 이 오브젝트를 유지한다
            DontDestroyOnLoad(gameObject);

            // Fusion 씬 매니저가 알려 주는 진행률을 받는다
            GameSceneManager.ProgressChanged += OnFusionProgress;

            // 씬을 바꿀 권한이 있는 쪽(방장)만 다음 씬 로드를 요청한다
            if (manager.runner.IsSceneAuthority)
            {
                // 로딩 씬 로드가 끝난 뒤에 Play 로드를 요청하는 코루틴을 시작한다
                StartCoroutine(LoadNextWithFusion());
            }
        }
        // 방에 없으면 이 컴퓨터에서만 다음 씬을 읽는다
        else
        {
            // 다음 씬을 비동기로 읽기 시작한다
            loadOperation = SceneManager.LoadSceneAsync(nextSceneName);

            // 게이지가 가득 차기 전에는 씬을 켜지 못하게 막는다 (progress 가 0.9 에서 대기한다)
            loadOperation.allowSceneActivation = false;
        }
    }

    // 매 프레임 슬라이더 게이지를 목표 진행률 쪽으로 움직이는 함수
    private void Update()
    {
        // 로컬 비동기 로드가 진행 중이면 그 진행률을 목표로 삼는다
        if (loadOperation != null)
        {
            // Unity 는 활성화 전에 진행률을 0.9 에서 멈추므로, 0.9 를 100% 로 환산한다
            float loadTarget = Mathf.Clamp01(loadOperation.progress / 0.9f);

            // 목표가 뒤로 줄어들지 않게, 더 커진 경우만 반영한다
            if (loadTarget > target)
            {
                // 슬라이더가 따라갈 목표를 갱신한다
                target = loadTarget;
            }
        }

        // 화면에 보이는 값을 목표까지 일정한 속도로 이동시킨다
        shown = Mathf.MoveTowards(shown, target, Time.deltaTime / FILL_SECONDS);

        // 슬라이더 값에 넣으면 Fill 막대가 그만큼 차오른다
        gauge.value = shown;

        // 0~100 정수 퍼센트로 바꿔 글자에 넣는다
        percentText.text = Mathf.RoundToInt(shown * 100f) + "%";

        // 로컬 로드가 준비됐고 게이지도 가득 찼는지 확인한다
        if (loadOperation != null && loadOperation.progress >= 0.9f && shown >= 0.999f)
        {
            // 이제 다음 씬을 화면에 켜도 된다고 허용한다
            loadOperation.allowSceneActivation = true;

            // 같은 작업을 다음 프레임에 또 허용하지 않도록 비운다
            loadOperation = null;
        }

        // 다음 씬이 이미 활성 씬이고 게이지도 가득 찼으면 로딩 화면을 치운다
        if (closing == false && shown >= 0.999f && SceneManager.GetActiveScene().name == nextSceneName)
        {
            // 치우는 중이라고 표시한다
            closing = true;

            // 게이지 캔버스를 삭제한다
            Destroy(gameObject);
        }
    }

    // 방장이 로딩 씬 진입이 끝난 뒤 게임 스타트 씬 로드를 요청하는 코루틴
    private IEnumerator LoadNextWithFusion()
    {
        // 러너를 가져온다
        NetworkRunner runner = NetworkManager.Instance.runner;

        // 로딩 씬을 여는 작업이 아직 끝나지 않았으면 한 프레임씩 기다린다
        while (runner != null && runner.IsSceneManagerBusy)
        {
            // 다음 프레임까지 기다린다
            yield return null;

            // 기다리는 동안 러너가 사라졌는지 다시 확인한다
            if (NetworkManager.Instance == null || NetworkManager.Instance.runner == null)
            {
                // 코루틴을 끝낸다
                yield break;
            }

            // 최신 러너를 다시 가져온다
            runner = NetworkManager.Instance.runner;
        }

        // 로딩 제목이 한순간 보이지 않고 바로 넘어가지 않도록 잠깐 유지한다
        yield return new WaitForSeconds(0.25f);

        // 기다리는 동안 연결이 끊겼는지 확인한다
        if (NetworkManager.Instance == null || NetworkManager.Instance.runner == null)
        {
            // 코루틴을 끝낸다
            yield break;
        }

        // 씬을 바꿀 러너를 다시 가져온다
        runner = NetworkManager.Instance.runner;

        // 로딩 씬 자체의 진행률이 남아 있으면 지우고, Play 로드부터 다시 재기 시작한다
        target = 0f;

        // 화면에 그려진 게이지도 다시 0 에서 차오르게 한다
        shown = 0f;

        // 방 안의 모두가 게임 스타트 씬(Play)으로 넘어가게 요청한다
        runner.LoadScene(nextSceneName, LoadSceneMode.Single, LocalPhysicsMode.None, true);
    }

    // Fusion 이 알려 준 씬 진행률을 게이지 목표로 저장하는 함수
    private void OnFusionProgress(float progress)
    {
        // Unity 는 씬이 켜지기 직전 진행률을 0.9 에서 멈추므로, 0.9 를 게이지 100% 로 본다
        float clamped = Mathf.Clamp01(progress / 0.9f);

        // 진행률이 커진 경우에만 목표를 올린다
        if (clamped > target)
        {
            // 슬라이더가 따라갈 목표를 갱신한다
            target = clamped;
        }
    }

    // 어떤 씬이든 로드가 끝나면 호출되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 우리가 기다리던 씬이 아니면 무시한다
        if (scene.name != nextSceneName)
        {
            // 함수를 끝낸다
            return;
        }

        // 씬이 열렸으면 게이지 목표를 가득(1)으로 맞춘다
        target = 1f;
    }

    // 이 오브젝트가 파괴될 때 구독을 해제하는 함수
    private void OnDestroy()
    {
        // 씬 로드 알림을 해제한다
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Fusion 진행률 알림을 해제한다
        GameSceneManager.ProgressChanged -= OnFusionProgress;
    }
}
