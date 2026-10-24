// 씬 로드 모드(Single)를 쓰기 위해 불러온다
using UnityEngine.SceneManagement;

// 씬에서 씬으로 이동하는 방법을 모아 둔 클래스
public static class SceneFlow
{
    // 로딩 씬이 게이지를 채운 뒤에 열어야 할 다음 씬 이름
    public static string NextSceneName;

    // 게이지 없이 씬을 바로 연다 (Bootstrap → Start, Start → Lobby)
    public static void Load(string sceneName)
    {
        // 요청한 이름의 씬을 지금 씬 대신 연다
        SceneManager.LoadScene(sceneName);
    }

    // 로딩 씬의 슬라이더 게이지를 거친 뒤 nextSceneName 씬을 연다
    public static void LoadThroughGauge(string nextSceneName)
    {
        // 로딩 씬이 읽을 수 있게 다음 씬 이름을 저장한다
        NextSceneName = nextSceneName;

        // 네트워크 매니저가 있는지 확인한다
        NetworkManager manager = NetworkManager.Instance;

        // 방에 들어가 있고, 이 컴퓨터가 씬을 바꿀 권한(방장)을 가졌는지 확인한다
        if (manager != null && manager.IsInRoom() && manager.runner.IsSceneAuthority)
        {
            // 방 안의 모든 사람이 같이 로딩 씬으로 넘어가게 한다
            manager.runner.LoadScene(SceneNames.Loading, LoadSceneMode.Single, LocalPhysicsMode.None, true);

            // 아래의 로컬 씬 전환은 실행하지 않는다
            return;
        }

        // 방에 들어간 손님은 방장이 씬을 바꿀 때까지 기다린다
        if (manager != null && manager.IsInRoom())
        {
            // 손님이 혼자 씬을 바꾸면 방장과 어긋나므로 여기서 끝낸다
            return;
        }

        // 아직 방에 없으면 이 컴퓨터에서만 로딩 씬을 연다
        SceneManager.LoadScene(SceneNames.Loading);
    }
}
