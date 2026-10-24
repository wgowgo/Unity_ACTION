using UnityEngine;

// P1은 파란 더미, P2는 빨간 더미를 쓴다. 히트 판정은 이 모델의 애니메이션을 보지 않는다.
[CreateAssetMenu(fileName = "FighterDummies", menuName = "Action/Fighter Dummies")]
public class FighterDummySet : ScriptableObject
{
    public GameObject PlayerOne;
    public GameObject PlayerTwo;
}
