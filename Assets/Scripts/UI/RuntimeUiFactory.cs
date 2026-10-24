// TextMeshPro 글자를 쓰기 위해 불러온다
using TMPro;
// Unity 기본 기능(GameObject, Sprite, Texture2D 등)을 불러온다
using UnityEngine;
// 슬라이더, 버튼, 캔버스 같은 UI 를 쓰기 위해 불러온다
using UnityEngine.UI;
// 새 Input System 으로 UI 클릭을 받기 위해 불러온다
using UnityEngine.InputSystem.UI;
// EventSystem 을 쓰기 위해 불러온다
using UnityEngine.EventSystems;

// 실행 중에 캔버스, 글자, 버튼, 슬라이더 게이지를 만드는 도구
public static class RuntimeUiFactory
{
    // 색만 있는 사각형 스프라이트. 이미지에 넣어야 화면에 색이 보인다
    private static Sprite whiteSprite;

    // 화면 전체를 덮는 오버레이 캔버스를 host 오브젝트에 붙이는 함수
    public static Canvas BuildOverlay(GameObject host)
    {
        // host 에 Canvas 를 붙인다 (붙이는 순간 Transform 이 RectTransform 으로 바뀐다)
        Canvas canvas = host.AddComponent<Canvas>();

        // 카메라 없이 화면 맨 위에 UI 를 그린다
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 로딩 게이지가 게임 화면보다 위에 있도록 순서를 높인다
        canvas.sortingOrder = 100;

        // 해상도가 달라도 UI 크기를 맞춰 주는 컴포넌트를 붙인다
        CanvasScaler scaler = host.AddComponent<CanvasScaler>();

        // 화면 크기에 비례해서 UI 가 커지고 작아지게 한다
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // 기준 해상도를 1920x1080 으로 정한다
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 가로와 세로를 반반 반영한다
        scaler.matchWidthOrHeight = 0.5f;

        // 버튼 클릭을 받을 수 있게 레이캐스터를 붙인다
        host.AddComponent<GraphicRaycaster>();

        // 만든 캔버스를 돌려준다
        return canvas;
    }

    // 버튼이 동작하려면 씬에 EventSystem 이 하나 있어야 한다
    public static void EnsureEventSystem()
    {
        // 이미 EventSystem 이 있는지 찾는다
        EventSystem existing = Object.FindAnyObjectByType<EventSystem>();

        // 이미 있으면 새로 만들지 않는다
        if (existing != null)
        {
            // 함수를 끝낸다
            return;
        }

        // EventSystem 과 새 Input System 용 입력 모듈을 가진 오브젝트를 만든다
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // 가운데 정렬 글자를 만드는 함수
    public static TMP_Text CreateLabel(Transform parent, string text, float fontSize, Vector2 position, Vector2 size)
    {
        // 글자 오브젝트를 만든다
        GameObject go = new GameObject("Label", typeof(RectTransform));

        // 부모 아래에 두고, 부모 스케일에 끌려가지 않게 로컬 값을 유지한다
        go.transform.SetParent(parent, false);

        // 위치와 크기를 다루기 위해 RectTransform 을 가져온다
        RectTransform rect = go.GetComponent<RectTransform>();

        // 기준점을 화면 가운데로 고정한다
        rect.anchorMin = new Vector2(0.5f, 0.5f);

        // 기준점의 반대쪽도 화면 가운데로 고정한다
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        // 글자 상자의 중심을 가운데로 둔다
        rect.pivot = new Vector2(0.5f, 0.5f);

        // 화면 가운데에서 얼마나 떨어질지 정한다
        rect.anchoredPosition = position;

        // 글자 상자의 가로, 세로 크기를 정한다
        rect.sizeDelta = size;

        // TextMeshPro 글자 컴포넌트를 붙인다
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();

        // 프로젝트에 설치된 TMP 기본 폰트를 넣는다
        label.font = TMP_Settings.defaultFontAsset;

        // 화면에 보일 문장을 넣는다
        label.text = text;

        // 글자 크기를 정한다
        label.fontSize = fontSize;

        // 가로, 세로 모두 가운데 정렬로 한다
        label.alignment = TextAlignmentOptions.Center;

        // 글자 색을 흰색으로 한다
        label.color = Color.white;

        // 글자 자체는 클릭을 가로채지 않게 한다
        label.raycastTarget = false;

        // 만든 글자 컴포넌트를 돌려준다
        return label;
    }

    // 가운데에 버튼을 만드는 함수
    public static Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size)
    {
        // 버튼 오브젝트를 만든다
        GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));

        // 부모 아래에 넣는다
        go.transform.SetParent(parent, false);

        // 위치와 크기를 다루기 위해 RectTransform 을 가져온다
        RectTransform rect = go.GetComponent<RectTransform>();

        // 기준점을 화면 가운데로 고정한다
        rect.anchorMin = new Vector2(0.5f, 0.5f);

        // 반대쪽 기준점도 화면 가운데로 고정한다
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        // 버튼의 중심을 가운데로 둔다
        rect.pivot = new Vector2(0.5f, 0.5f);

        // 화면 가운데에서의 위치를 정한다
        rect.anchoredPosition = position;

        // 버튼의 가로, 세로 크기를 정한다
        rect.sizeDelta = size;

        // 버튼 배경 이미지를 가져온다
        Image image = go.GetComponent<Image>();

        // 색이 보이도록 흰색 스프라이트를 넣는다
        image.sprite = GetWhiteSprite();

        // 이미지 타입을 단순 사각형으로 한다
        image.type = Image.Type.Simple;

        // 버튼 배경색을 어두운 회색으로 한다
        image.color = new Color(0.2f, 0.22f, 0.28f, 1f);

        // 버튼 컴포넌트를 가져온다
        Button button = go.GetComponent<Button>();

        // 버튼 안의 글자를 만들고, 버튼 크기에 맞춰 글자 상자를 채운다
        TMP_Text label = CreateLabel(go.transform, text, 36, Vector2.zero, size);

        // 글자 상자를 버튼 전체에 붙인다
        Stretch(label.rectTransform);

        // 만든 버튼을 돌려준다
        return button;
    }

    // 슬라이더를 게이지바로 만드는 함수. 드래그할 수 없고, 값 0~1 이 왼쪽에서 오른쪽으로 찬다
    public static Slider CreateGauge(Transform parent, Vector2 position, Vector2 size)
    {
        // 슬라이더 루트 오브젝트를 만든다
        GameObject root = new GameObject("Gauge", typeof(RectTransform), typeof(Slider));

        // 부모 아래에 넣는다
        root.transform.SetParent(parent, false);

        // 게이지 전체의 RectTransform 을 가져온다
        RectTransform rootRect = root.GetComponent<RectTransform>();

        // 기준점을 화면 가운데로 고정한다
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);

        // 반대쪽 기준점도 화면 가운데로 고정한다
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);

        // 게이지의 중심을 가운데로 둔다
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        // 화면 가운데에서의 위치를 정한다
        rootRect.anchoredPosition = position;

        // 게이지의 가로, 세로 크기를 정한다
        rootRect.sizeDelta = size;

        // 게이지 뒤쪽 배경 오브젝트를 만든다
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));

        // 배경을 게이지 루트 아래에 넣는다
        background.transform.SetParent(root.transform, false);

        // 배경 이미지를 가져온다
        Image backgroundImage = background.GetComponent<Image>();

        // 색이 보이도록 흰색 스프라이트를 넣는다
        backgroundImage.sprite = GetWhiteSprite();

        // 배경색을 어두운 색으로 한다
        backgroundImage.color = new Color(0.15f, 0.16f, 0.2f, 1f);

        // 배경은 클릭을 받지 않는다
        backgroundImage.raycastTarget = false;

        // 배경을 게이지 칸 전체에 붙인다
        Stretch(background.GetComponent<RectTransform>());

        // 채워지는 영역(여백을 둘 칸)을 만든다
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));

        // 채움 영역을 게이지 루트 아래에 넣는다
        fillArea.transform.SetParent(root.transform, false);

        // 채움 영역의 RectTransform 을 가져온다
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();

        // 채움 영역을 게이지 안쪽에 붙인다
        Stretch(fillAreaRect);

        // 위, 아래, 왼쪽, 오른쪽에 8 픽셀 여백을 준다
        fillAreaRect.offsetMin = new Vector2(8f, 8f);

        // 오른쪽과 위쪽 여백은 음수로 안쪽을 줄인다
        fillAreaRect.offsetMax = new Vector2(-8f, -8f);

        // 실제로 차오르는 주황 막대 오브젝트를 만든다
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));

        // 주황 막대를 채움 영역 아래에 넣는다
        fill.transform.SetParent(fillArea.transform, false);

        // 주황 막대 이미지를 가져온다
        Image fillImage = fill.GetComponent<Image>();

        // 색이 보이도록 흰색 스프라이트를 넣는다
        fillImage.sprite = GetWhiteSprite();

        // 게이지가 차오르는 색을 주황색으로 한다
        fillImage.color = new Color(0.95f, 0.55f, 0.15f, 1f);

        // 막대는 클릭을 받지 않는다
        fillImage.raycastTarget = false;

        // 막대를 채움 영역 전체에 붙인다. 슬라이더가 이 Rect 의 가로 비율을 줄여 가며 게이지처럼 보여 준다
        Stretch(fill.GetComponent<RectTransform>());

        // 슬라이더 컴포넌트를 가져온다
        Slider slider = root.GetComponent<Slider>();

        // 차오를 대상을 주황 막대로 지정한다
        slider.fillRect = fill.GetComponent<RectTransform>();

        // 슬라이더가 색을 바꿀 때 쓸 그래픽을 배경으로 지정한다
        slider.targetGraphic = backgroundImage;

        // 왼쪽에서 오른쪽으로 차게 한다
        slider.direction = Slider.Direction.LeftToRight;

        // 게이지의 최소값을 0(빈 상태)으로 한다
        slider.minValue = 0f;

        // 게이지의 최대값을 1(가득 찬 상태)로 한다
        slider.maxValue = 1f;

        // 시작할 때는 빈 상태로 둔다
        slider.value = 0f;

        // 정수만 쓰지 않고 소수점 진행률을 그대로 쓴다
        slider.wholeNumbers = false;

        // 유저가 드래그해서 게이지를 마음대로 움직이지 못하게 한다
        slider.interactable = false;

        // 색이 눌림에 따라 바뀌지 않게 한다
        slider.transition = Selectable.Transition.None;

        // 키보드 포커스를 받지 않도록 내비게이션을 끈다
        // "키보드 포커스"는 Tab 키 등으로 UI 콘트롤(버튼, 입력칸 등)에 초점을 옮겨서 키보드로 조작할 수 있는 상태를 말한다.
        // 내비게이션을 끄면 이 슬라이더(게이지)는 Tab 키 등으로 선택되지 않아 키보드 포커스를 받지 않는다.
        Navigation navigation = slider.navigation;

        // 내비게이션 모드를 없음으로 바꾼다
        navigation.mode = Navigation.Mode.None;

        // 바꾼 내비게이션을 슬라이더에 다시 넣는다
        slider.navigation = navigation;

        // 만든 슬라이더를 돌려준다
        return slider;
    }

    // RectTransform 을 부모 칸 전체에 붙이는 함수
    private static void Stretch(RectTransform rect)
    {
        // 왼쪽 아래 기준점을 부모의 왼쪽 아래로 한다
        rect.anchorMin = Vector2.zero;

        // 오른쪽 위 기준점을 부모의 오른쪽 위로 한다
        rect.anchorMax = Vector2.one;

        // 피벗을 가운데로 둔다
        rect.pivot = new Vector2(0.5f, 0.5f);

        // 왼쪽, 아래 여백을 0 으로 한다
        rect.offsetMin = Vector2.zero;

        // 오른쪽, 위 여백을 0 으로 한다
        rect.offsetMax = Vector2.zero;

        // 추가 위치 이동은 없다
        rect.anchoredPosition = Vector2.zero;
    }

    // 1x1 흰색 스프라이트를 만들어 재사용하는 함수
    private static Sprite GetWhiteSprite()
    {
        // 이미 만들어 둔 스프라이트가 있으면 그것을 돌려준다
        if (whiteSprite != null)
        {
            // 저장해 둔 스프라이트를 돌려준다
            return whiteSprite;
        }

        // 픽셀 하나짜리 텍스처를 만든다
        Texture2D texture = new Texture2D(1, 1);

        // 그 픽셀을 흰색으로 칠한다
        texture.SetPixel(0, 0, Color.white);

        // 칠한 색을 텍스처에 반영한다
        texture.Apply();

        // 씬을 바꿔도 이 텍스처가 함부로 지워지지 않게 표시한다
        texture.hideFlags = HideFlags.HideAndDontSave;

        // 텍스처 전체를 스프라이트로 만든다
        whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // 스프라이트도 함부로 지워지지 않게 표시한다
        whiteSprite.hideFlags = HideFlags.HideAndDontSave;

        // 만든 스프라이트를 돌려준다
        return whiteSprite;
    }
}
