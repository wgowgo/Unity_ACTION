// 씬이 열릴 때 알림을 받기 위해 불러온다
using UnityEngine.SceneManagement;
// Unity 기본 기능(GameObject, MonoBehaviour 등)을 불러온다
using UnityEngine;

// 게임 실행 시 씬 이름에 맞는 스크립트를 자동으로 붙여 주는 클래스
// 씬 파일 안에 스크립트를 미리 넣어 두지 않아도 흐름이 동작하게 한다
public static class SceneEntryPoint
{
    // 씬이 로드되기 전에 한 번 호출되어, 씬 로드 알림을 등록한다
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        // 이전에 등록된 알림이 있으면 빼서, 두 번 등록되는 것을 막는다
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 씬이 열릴 때마다 OnSceneLoaded 가 호출되도록 등록한다
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 씬 로드가 끝났을 때 호출되는 함수
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 켜진 씬이 부트스트랩이면 초기화 스크립트를 붙인다
        if (scene.name == SceneNames.Bootstrap)
        {
            // 이미 붙어 있지 않을 때만 만든다
            Ensure<GameBootstrap>();
        }

        // 켜진 씬이 스타트(타이틀)이면 스타트 화면 스크립트를 붙인다
        if (scene.name == SceneNames.Start)
        {
            // 이미 붙어 있지 않을 때만 만든다
            Ensure<StartUI>();
        }

        // 켜진 씬이 로딩이면 슬라이더 게이지 스크립트를 붙인다
        if (scene.name == SceneNames.Loading)
        {
            // 이미 붙어 있지 않을 때만 만든다
            Ensure<LoadingUI>();
        }

        // 켜진 씬이 플레이(게임 스타트)이면 도착 화면 스크립트를 붙인다
        if (scene.name == SceneNames.Play)
        {
            // 이미 붙어 있지 않을 때만 만든다
            Ensure<PlayStartUI>();
        }
    }

    // 해당 스크립트가 씬에 없으면 새 오브젝트를 만들어 붙이는 함수
    private static void Ensure<T>() where T : Component
    {
        // 이미 같은 스크립트가 있는지 찾는다
        T existing = Object.FindAnyObjectByType<T>();

        // 이미 있으면 또 만들지 않고 끝낸다
        if (existing != null)
        {
            // 함수를 끝낸다
            return;
        }

        // 스크립트 이름으로 빈 오브젝트를 만든다
        GameObject host = new GameObject(typeof(T).Name);

        // 그 오브젝트에 스크립트를 붙인다 (Awake 가 바로 실행된다)
        host.AddComponent<T>();
    }
}
