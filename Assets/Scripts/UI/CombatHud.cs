using TMPro; // 텍스트 메시 프로 텍스트 컴포넌트를 쓰기 위한 네임스페이스
using UnityEngine; // MonoBehaviour, GameObject, Color, Mathf 등 유니티 기본 API
using UnityEngine.UI; // Slider, Button, Image 같은 uGUI 컴포넌트

// 로컬 플레이어의 체력, 게이지, 경기 상태, 결과 버튼을 그린다.
public class CombatHud : MonoBehaviour // 전투 HUD를 런타임에 만들고 매 프레임 갱신하는 컴포넌트
{ // 클래스 본문 시작
    TMP_Text phaseText; // 라운드, 카운트다운, 승패 문구를 표시하는 중앙 상단 텍스트
    TMP_Text hintText; // 조작키 안내를 표시하는 하단 텍스트
    Slider healthP1; // 왼쪽(1번 슬롯) 플레이어 체력 바
    Slider healthP2; // 오른쪽(2번 슬롯) 플레이어 체력 바
    Slider gauge; // 로컬 플레이어 궁극기 게이지 바
    GameObject resultRoot; // 경기 결과 패널 루트. 결과 페이즈에서만 켠다
    TMP_Text resultText; // 승패 제목과 양측 스코어, 개인 전적을 담는 텍스트
    Button rematchButton; // 재대결을 요청하는 버튼
    Button lobbyButton; // 로비로 돌아가기를 요청하는 버튼
    GameObject pauseRoot; // 일시정지 패널 루트. 입력이 막혀 있을 때 표시한다

    void Start() // HUD 위젯을 한 번 생성하고 초기 상태를 맞춘다
    { // Start 본문 시작
        RuntimeUiFactory.EnsureEventSystem(); // 버튼 클릭이 동작하도록 이벤트 시스템을 보장한다
        phaseText = RuntimeUiFactory.CreateLabel(transform, "", 42, new Vector2(0f, 460f), new Vector2(900f, 80f)); // 화면 상단 중앙에 페이즈 라벨을 만든다
        hintText = RuntimeUiFactory.CreateLabel(transform, "A D Move   W Jump   S Crouch   LMB Light   E Heavy   RMB Guard   Shift Dash   F Counter   Q Ultimate", 22, new Vector2(0f, -500f), new Vector2(1700f, 50f)); // 화면 하단에 조작 안내 문구를 만든다

        healthP1 = RuntimeUiFactory.CreateGauge(transform, new Vector2(-430f, 470f), new Vector2(520f, 36f)); // 왼쪽 상단에 1번 플레이어 체력 바를 만든다
        healthP2 = RuntimeUiFactory.CreateGauge(transform, new Vector2(430f, 470f), new Vector2(520f, 36f)); // 오른쪽 상단에 2번 플레이어 체력 바를 만든다
        gauge = RuntimeUiFactory.CreateGauge(transform, new Vector2(0f, -450f), new Vector2(420f, 22f)); // 하단 중앙에 궁극기 게이지 바를 만든다
        Tint(healthP1, new Color(0.25f, 0.55f, 0.95f, 1f)); // 1번 체력 바를 파란색으로 칠한다
        Tint(healthP2, new Color(0.9f, 0.28f, 0.25f, 1f)); // 2번 체력 바를 빨간색으로 칠한다
        Tint(gauge, new Color(0.95f, 0.75f, 0.2f, 1f)); // 궁극기 게이지를 노란색으로 칠한다

        resultRoot = new GameObject("Result", typeof(RectTransform)); // 결과 패널용 빈 UI 오브젝트를 만든다
        resultRoot.transform.SetParent(transform, false); // 결과 패널을 이 HUD의 자식으로 붙인다
        resultText = RuntimeUiFactory.CreateLabel(resultRoot.transform, "", 48, new Vector2(0f, 80f), new Vector2(1100f, 220f)); // 결과 문구 영역을 만든다
        rematchButton = RuntimeUiFactory.CreateButton(resultRoot.transform, "Rematch", new Vector2(-160f, -80f), new Vector2(280f, 70f)); // 재대결 버튼을 왼쪽에 만든다
        lobbyButton = RuntimeUiFactory.CreateButton(resultRoot.transform, "Lobby", new Vector2(160f, -80f), new Vector2(280f, 70f)); // 로비 버튼을 오른쪽에 만든다
        rematchButton.onClick.AddListener(OnRematch); // 재대결 버튼 클릭 시 재대결 요청을 보낸다
        lobbyButton.onClick.AddListener(OnLobby); // 로비 버튼 클릭 시 로비 복귀 요청을 보낸다
        resultRoot.SetActive(false); // 경기 시작 전에는 결과 패널을 숨긴다

        pauseRoot = new GameObject("Pause", typeof(RectTransform)); // 일시정지 패널용 빈 UI 오브젝트를 만든다
        pauseRoot.transform.SetParent(transform, false); // 일시정지 패널을 이 HUD의 자식으로 붙인다
        RuntimeUiFactory.CreateLabel(pauseRoot.transform, "Paused", 54, new Vector2(0f, 80f), new Vector2(800f, 100f)); // 일시정지 제목 텍스트를 만든다
        Button resume = RuntimeUiFactory.CreateButton(pauseRoot.transform, "Resume", new Vector2(0f, -20f), new Vector2(320f, 70f)); // 재개 버튼을 만든다
        Button leave = RuntimeUiFactory.CreateButton(pauseRoot.transform, "Leave Room", new Vector2(0f, -110f), new Vector2(320f, 70f)); // 방 나가기 버튼을 만든다
        resume.onClick.AddListener(() => FusionInputProvider.Blocked = false); // 재개 버튼은 입력 차단을 해제한다
        leave.onClick.AddListener(SessionFlow.LeaveMatch); // 나가기 버튼은 현재 매치를 떠난다
        pauseRoot.SetActive(false); // 시작 시에는 일시정지 패널을 숨긴다
    } // Start 본문 끝

    void Update() // 매 프레임 체력, 게이지, 페이즈 문구, 패널 표시를 갱신한다
    { // Update 본문 시작
        if (pauseRoot != null) // 일시정지 패널이 만들어져 있을 때만 처리한다
        { // 일시정지 표시 분기 시작
            pauseRoot.SetActive(FusionInputProvider.Blocked); // 입력이 막혀 있으면 일시정지 패널을 켜고, 아니면 끈다
        } // 일시정지 표시 분기 끝

        if (string.IsNullOrEmpty(SessionFlow.SpawnError) == false) // 스폰 오류 메시지가 있으면
        { // 스폰 오류 표시 분기 시작
            phaseText.text = SessionFlow.SpawnError; // 페이즈 텍스트에 스폰 오류를 그대로 보여 준다
        } // 스폰 오류 표시 분기 끝

        MatchStateManager match = MatchStateManager.Active; // 현재 활성 경기 상태 관리자를 가져온다
        PlayerAvatar local = LocalAvatar(); // 이 클라이언트에서 조작하는 로컬 아바타를 찾는다
        PlayerAvatar left = FighterBySlot(0); // 왼쪽 슬롯(0) 파이터를 찾는다
        PlayerAvatar right = FighterBySlot(1); // 오른쪽 슬롯(1) 파이터를 찾는다
        if (healthP1 != null) // 체력 바가 생성되어 있을 때만 수치를 넣는다
        { // 게이지 값 갱신 분기 시작
            healthP1.value = Health01(left); // 왼쪽 체력 바를 0~1 비율로 맞춘다
            healthP2.value = Health01(right); // 오른쪽 체력 바를 0~1 비율로 맞춘다
            gauge.value = local != null ? Mathf.Clamp01(local.Gauge / GameConfig.UltimateMax) : 0f; // 로컬 궁극기 게이지를 최대치 대비 비율로 넣고, 없으면 0
        } // 게이지 값 갱신 분기 끝

        if (match == null) // 아직 경기 상태 관리자가 없으면 대기 화면으로 둔다
        { // 스폰 대기 분기 시작
            if (string.IsNullOrEmpty(SessionFlow.SpawnError)) // 따로 보여줄 스폰 오류가 없을 때만
            { // 대기 문구 분기 시작
                phaseText.text = "Spawning players"; // 플레이어 스폰 중이라는 문구를 표시한다
            } // 대기 문구 분기 끝

            resultRoot.SetActive(false); // 경기가 없으므로 결과 패널은 끈다
            return; // 이후 페이즈 갱신은 건너뛴다
        } // 스폰 대기 분기 끝

        phaseText.text = PhaseLabel(match); // 현재 페이즈에 맞는 상단 문구로 바꾼다
        bool showResult = match.Phase == MatchPhase.Results; // 결과 페이즈일 때만 결과 패널을 연다
        resultRoot.SetActive(showResult); // 결과 패널 표시 여부를 페이즈에 맞춘다
        if (showResult) // 결과 패널이 보이는 동안
        { // 결과 문구 갱신 분기 시작
            resultText.text = BuildResult(match); // 승패, 스코어, 개인 전적 문자열을 채운다
        } // 결과 문구 갱신 분기 끝
    } // Update 본문 끝

    static float Health01(PlayerAvatar avatar) // 아바타 체력을 슬라이더용 0~1 값으로 바꾼다
    { // Health01 본문 시작
        if (avatar == null || avatar.MaxHealth <= 0f) // 아바타가 없거나 최대 체력이 0 이하면 나눗셈을 피한다
        { // 체력 없음 분기 시작
            return 0f; // 빈 체력 바로 표시한다
        } // 체력 없음 분기 끝

        return Mathf.Clamp01(avatar.Health / avatar.MaxHealth); // 현재 체력을 최대 체력으로 나눈 뒤 0~1로 자른다
    } // Health01 본문 끝

    static PlayerAvatar FighterBySlot(int slot) // 스폰 슬롯 번호와 일치하는 파이터를 찾는다
    { // FighterBySlot 본문 시작
        for (int i = 0; i < PlayerAvatar.All.Count; i++) // 씬에 있는 모든 아바타를 순서대로 본다
        { // 아바타 탐색 루프 시작
            PlayerAvatar avatar = PlayerAvatar.All[i]; // i번째 아바타를 꺼낸다
            if (avatar != null && avatar.SpawnIndex == slot) // 살아 있고 요청한 슬롯이면
            { // 슬롯 일치 분기 시작
                return avatar; // 그 아바타를 바로 반환한다
            } // 슬롯 일치 분기 끝
        } // 아바타 탐색 루프 끝

        return null; // 해당 슬롯 파이터가 없으면 null
    } // FighterBySlot 본문 끝

    static PlayerAvatar LocalAvatar() // 이 러너의 로컬 플레이어가 소유한 아바타를 찾는다
    { // LocalAvatar 본문 시작
        if (NetworkManager.Instance == null || NetworkManager.Instance.runner == null) // 네트워크 매니저나 러너가 없으면
        { // 네트워크 없음 분기 시작
            return null; // 로컬 아바타를 특정할 수 없다
        } // 네트워크 없음 분기 끝

        return PlayerAvatar.Find(NetworkManager.Instance.runner.LocalPlayer); // 로컬 플레이어 참조로 아바타를 조회한다
    } // LocalAvatar 본문 끝

    static string PhaseLabel(MatchStateManager match) // 경기 페이즈와 남은 시간에 따라 상단 문구를 만든다
    { // PhaseLabel 본문 시작
        float remain = 0f; // 페이즈 남은 시간(초). 타이머가 없으면 0
        float? left = match.PhaseTimer.RemainingTime(match.Runner); // Fusion 타이머에서 남은 시간을 읽는다
        if (left.HasValue) // 남은 시간 값이 있으면
        { // 남은 시간 반영 분기 시작
            remain = left.Value; // 그 값을 문구 계산에 쓴다
        } // 남은 시간 반영 분기 끝

        int round = Mathf.Max(1, match.DisplayRound); // 표시 라운드는 최소 1로 맞춘다
        string score = "P1  " + match.LeftWins + "   -   " + match.RightWins + "  P2"; // 좌우 라운드 승수 문자열
        if (match.Phase == MatchPhase.Countdown) // 라운드 시작 카운트다운이면
        { // 카운트다운 문구 분기 시작
            return remain <= 1f ? "FIGHT" : "ROUND " + round; // 마지막 1초는 FIGHT, 그 전에는 라운드 번호
        } // 카운트다운 문구 분기 끝

        if (match.Phase == MatchPhase.Playing) // 실제 대전 중이면
        { // 진행 중 문구 분기 시작
            int seconds = Mathf.CeilToInt(remain); // 남은 시간을 초 단위로 올림한다
            return score + "\n" + seconds.ToString("00"); // 스코어 아래 줄에 두 자리 남은 초를 붙인다
        } // 진행 중 문구 분기 끝

        if (match.Phase == MatchPhase.Finishing) // 라운드가 끝나는 연출 중이면
        { // 종료 연출 문구 분기 시작
            if (match.MatchDecided) // 매치 전체가 이미 결판 났으면
            { // 매치 결판 문구 분기 시작
                return LocalWon(match) ? "YOU WIN" : match.IsDraw ? "DRAW" : "YOU LOSE"; // 로컬 기준 승, 무, 패
            } // 매치 결판 문구 분기 끝

            if (match.EndReason == RoundEndReason.Time) // 시간 초과로 라운드가 끝났으면
            { // 시간 초과 문구 분기 시작
                return "TIME OVER"; // 타임 오버를 표시한다
            } // 시간 초과 문구 분기 끝

            return match.IsDraw ? "DRAW" : "KO"; // 그 외에는 무승부 또는 KO
        } // 종료 연출 문구 분기 끝

        return ""; // Results 등 나머지 페이즈는 상단 문구를 비운다
    } // PhaseLabel 본문 끝

    static string BuildResult(MatchStateManager match) // 결과 패널에 넣을 여러 줄 전적 문자열을 만든다
    { // BuildResult 본문 시작
        string title = match.LeftWins == match.RightWins ? "DRAW" : LocalWon(match) ? "YOU WIN" : "YOU LOSE"; // 스코어가 같으면 무승부, 아니면 로컬 승패
        string body = title + "\n" + match.LeftWins + " - " + match.RightWins + "\n"; // 제목 다음 줄에 좌우 라운드 스코어를 붙인다
        for (int i = 0; i < PlayerAvatar.All.Count; i++) // 참가한 모든 아바타의 개인 전적을 이어 붙인다
        { // 전적 나열 루프 시작
            PlayerAvatar avatar = PlayerAvatar.All[i]; // i번째 아바타를 꺼낸다
            body += "P" + avatar.Object.InputAuthority.PlayerId // 입력 권한을 가진 플레이어 ID
                + "  HP " + Mathf.CeilToInt(avatar.Health) // 남은 체력을 정수로 올림해 표시
                + "  K " + avatar.Kills // 킬 수
                + "  D " + avatar.Deaths + "\n"; // 데스 수와 줄바꿈
        } // 전적 나열 루프 끝

        return body; // 완성된 결과 문구를 반환한다
    } // BuildResult 본문 끝

    static bool LocalWon(MatchStateManager match) // 로컬 플레이어가 라운드 승수에서 이겼는지 판정한다
    { // LocalWon 본문 시작
        PlayerAvatar local = LocalAvatar(); // 로컬 아바타를 다시 찾는다
        if (local == null) // 로컬 아바타가 없으면
        { // 로컬 없음 분기 시작
            return false; // 승리로 보지 않는다
        } // 로컬 없음 분기 끝

        if (local.SpawnIndex == 0) // 로컬이 왼쪽 슬롯이면
        { // 왼쪽 슬롯 판정 분기 시작
            return match.LeftWins > match.RightWins; // 왼쪽 승수가 더 많아야 이긴 것이다
        } // 왼쪽 슬롯 판정 분기 끝

        return match.RightWins > match.LeftWins; // 오른쪽 슬롯이면 오른쪽 승수가 더 많아야 이긴 것이다
    } // LocalWon 본문 끝

    static void Tint(Slider slider, Color color) // 슬라이더 채움 이미지의 색을 바꾼다
    { // Tint 본문 시작
        if (slider.fillRect == null) // 채움 영역이 없으면
        { // 채움 없음 분기 시작
            return; // 색을 바꿀 대상이 없으므로 끝낸다
        } // 채움 없음 분기 끝

        Image image = slider.fillRect.GetComponent<Image>(); // 채움 Rect에서 Image를 찾는다
        if (image != null) // Image가 붙어 있으면
        { // 색 적용 분기 시작
            image.color = color; // 요청한 색으로 칠한다
        } // 색 적용 분기 끝
    } // Tint 본문 끝

    static void OnRematch() // 재대결 버튼 클릭 처리
    { // OnRematch 본문 시작
        if (MatchStateManager.Active != null) // 활성 경기 관리자가 있을 때만
        { // 재대결 요청 분기 시작
            MatchStateManager.Active.RequestRematch(); // 재대결을 요청한다
        } // 재대결 요청 분기 끝
    } // OnRematch 본문 끝

    static void OnLobby() // 로비 버튼 클릭 처리
    { // OnLobby 본문 시작
        if (MatchStateManager.Active != null) // 활성 경기 관리자가 있을 때만
        { // 로비 요청 분기 시작
            MatchStateManager.Active.RequestLobby(); // 로비 복귀를 요청한다
        } // 로비 요청 분기 끝
    } // OnLobby 본문 끝
} // 클래스 본문 끝
