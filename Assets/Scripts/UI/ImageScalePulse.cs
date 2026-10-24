// Unity 기본 기능(MonoBehaviour, Transform, Mathf 등)을 불러온다
using UnityEngine;

// 이미지(또는 아무 오브젝트)의 크기를 최소~최대 스케일 사이에서 커졌다 작아졌다 반복하는 스크립트
// 이 스크립트를 이미지 오브젝트에 붙이고, Min Scale / Max Scale 만 바꾸면 된다
public class ImageScalePulse : MonoBehaviour
{
    // Inspector 에 "스케일 범위" 제목을 표시한다
    [Header("스케일 범위")]
    // 작아졌을 때의 배율. 1 이면 원래 크기, 0.8 이면 80% 크기
    public float minScale = 0.85f;
    // 커졌을 때의 배율. 1.15 이면 원래보다 15% 큰 크기
    public float maxScale = 1.15f;

    // Inspector 에 "속도" 제목을 표시한다
    [Header("속도")]
    // 작은 크기에서 큰 크기까지 가는 데 걸리는 시간(초). 작을수록 빨리 반복된다
    public float halfCycleSeconds = 0.6f;

    // 크기를 바꿀 대상. 비워 두면 이 스크립트가 붙어 있는 오브젝트를 쓴다
    private Transform target;

    // 시작할 때 원래 스케일을 기억해 둔다. 범위 배율은 이 값에 곱한다
    private Vector3 baseScale;

    // 오브젝트가 켜질 때 한 번 호출되는 함수
    private void Awake()
    {
        // 크기를 바꿀 대상을 이 오브젝트로 정한다
        target = transform;

        // 지금 Inspector 에 잡혀 있는 스케일을 기준 크기로 저장한다
        baseScale = target.localScale;
    }

    // 매 프레임 호출되는 함수
    private void Update()
    {
        // 한 번 왕복(작음→큼→작음)에 걸리는 시간. 편도 시간의 두 배다
        float cycle = halfCycleSeconds * 2f;

        // 편도 시간이 0 이하면 나눗셈을 피하고 최대 크기로 고정한다
        if (cycle <= 0f)
        {
            // 기준 크기에 최대 배율을 곱한 스케일을 넣는다
            target.localScale = baseScale * maxScale;

            // 이 프레임은 여기서 끝낸다
            return;
        }

        // 0~1 을 왕복하는 값. 0 이면 최소, 1 이면 최대, 다시 0 으로 돌아온다
        float pingPong = Mathf.PingPong(Time.time / halfCycleSeconds, 1f);

        // 왕복 값을 최소 배율과 최대 배율 사이의 배율로 바꾼다
        float scale = Mathf.Lerp(minScale, maxScale, pingPong);

        // 기준 크기에 그 배율을 곱해, 설정한 범위 안에서만 커졌다 작아지게 한다
        target.localScale = baseScale * scale;
    }
}
