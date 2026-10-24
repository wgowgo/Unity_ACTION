// 씬 파일 이름을 한곳에 모아 두는 클래스
// 흐름: Bootstrap → Start → Lobby → Loading(슬라이더 게이지) → Play(게임 스타트 씬)
public static class SceneNames
{
    // 맨 처음 켜지는 초기화 씬 이름 (Assets/Scenes/Bootstrap.unity)
    public const string Bootstrap = "Bootstrap";

    // 타이틀 씬 이름. START 버튼을 누르면 로비로 간다
    public const string Start = "Start";

    // 방을 만들거나 들어가는 로비 씬 이름
    public const string Lobby = "Lobby";

    // 슬라이더를 게이지바로 쓰는 로딩 씬 이름
    public const string Loading = "Loading";

    // 로딩이 끝난 뒤 게임이 시작되는 씬 이름 (게임 스타트 씬)
    public const string Play = "Play";
}
