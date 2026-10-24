// 진행률 숫자를 밖으로 전달하기 위해 불러온다
using System;
// Fusion 씬 참조(SceneRef)를 쓰기 위해 불러온다
using Fusion;

// Fusion 기본 씬 매니저를 그대로 쓰면서, 로딩 진행률만 밖으로 알려 주는 클래스
public class GameSceneManager : NetworkSceneManagerDefault
{
    // 씬 로딩 진행률(0~1)이 바뀔 때 구독자에게 알려 주는 이벤트
    public static event Action<float> ProgressChanged;

    // Fusion 이 씬을 읽는 동안 진행률이 바뀔 때마다 호출되는 함수
    protected override void OnLoadSceneProgress(SceneRef sceneRef, float progress)
    {
        // 부모의 로그 출력은 그대로 실행한다
        base.OnLoadSceneProgress(sceneRef, progress);

        // 구독자가 있을 때만 진행률을 전달한다
        if (ProgressChanged != null)
        {
            // 로딩 화면 슬라이더가 이 값을 게이지로 사용한다
            ProgressChanged.Invoke(progress);
        }
    }
}
