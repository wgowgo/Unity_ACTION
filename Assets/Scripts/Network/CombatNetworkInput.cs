using Fusion;

// 2.5D 대전 입력. Horizontal 은 화면 기준 왼쪽 -1, 오른쪽 1이다. 전진은 시뮬레이션에서 상대 방향으로 해석한다.
public struct CombatNetworkInput : INetworkInput
{
    public sbyte Horizontal;
    public NetworkButtons Buttons;

    public bool Held(ActionButton button)
    {
        return Buttons.IsSet(button);
    }
}
