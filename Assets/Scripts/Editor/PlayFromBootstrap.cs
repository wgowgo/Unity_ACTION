// 에디터에서 씬 에셋을 다루기 위해 불러온다
using UnityEditor;
// 플레이 버튼을 누를 때 시작 씬을 정하기 위해 불러온다
using UnityEditor.SceneManagement;
// Unity 기본 기능(Debug 등)을 불러온다
using UnityEngine;

// 프로젝트가 열리거나 스크립트가 컴파일되면, 플레이가 항상 Bootstrap 씬부터 시작되게 맞추는 도구
[InitializeOnLoad]
public static class PlayFromBootstrap
{
    // Bootstrap 씬 파일 위치
    private const string BOOTSTRAP_PATH = "Assets/Scenes/Bootstrap.unity";

    // 에디터가 이 클래스를 불러올 때 한 번 실행되는 함수
    static PlayFromBootstrap()
    {
        // Bootstrap 씬 에셋을 찾는다
        SceneAsset bootstrap = AssetDatabase.LoadAssetAtPath<SceneAsset>(BOOTSTRAP_PATH);

        // 씬 파일이 있을 때만 시작 씬을 바꾼다
        if (bootstrap != null && EditorSceneManager.playModeStartScene != bootstrap)
        {
            // 어떤 씬을 열어 둔 상태여도 플레이는 Bootstrap 부터 시작된다
            EditorSceneManager.playModeStartScene = bootstrap;
        }

        // 빌드에 넣을 씬 목록을 흐름 순서대로 만든다
        EditorBuildSettingsScene[] desired = new EditorBuildSettingsScene[]
        {
            // 1. 초기화
            new EditorBuildSettingsScene(BOOTSTRAP_PATH, true),
            // 2. 타이틀
            new EditorBuildSettingsScene("Assets/Scenes/Start.unity", true),
            // 3. 로비
            new EditorBuildSettingsScene("Assets/Scenes/Lobby.unity", true),
            // 4. 슬라이더 게이지 로딩
            new EditorBuildSettingsScene("Assets/Scenes/Loading.unity", true),
            // 5. 게임 스타트 씬
            new EditorBuildSettingsScene("Assets/Scenes/Play.unity", true),
        };

        // 지금 빌드 목록을 가져온다
        EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;

        // 이미 같은 순서인지 확인한다
        if (SameScenes(current, desired) == false)
        {
            // 빌드 목록을 흐름 순서로 덮어쓴다. Fusion 이 씬 번호로 씬을 찾으려면 여기 등록되어 있어야 한다
            EditorBuildSettings.scenes = desired;
        }
    }

    // 두 빌드 씬 목록이 같은지 비교하는 함수
    private static bool SameScenes(EditorBuildSettingsScene[] left, EditorBuildSettingsScene[] right)
    {
        // 개수가 다르면 같은 목록이 아니다
        if (left == null || left.Length != right.Length)
        {
            // 다름을 돌려준다
            return false;
        }

        // 앞에서부터 하나씩 비교한다
        for (int i = 0; i < left.Length; i++)
        {
            // 경로가 다르거나 켜짐 상태가 다르면 같은 목록이 아니다
            if (left[i].path != right[i].path || left[i].enabled != right[i].enabled)
            {
                // 다름을 돌려준다
                return false;
            }
        }

        // 전부 같으면 참을 돌려준다
        return true;
    }
}
