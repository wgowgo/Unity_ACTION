using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

// 측면 대전 조작을 모아 Fusion 틱 입력으로 넘긴다. 카메라 회전은 넣지 않는다.
public class FusionInputProvider : RunnerCallbacks
{
    public static FusionInputProvider Instance { get; private set; }

    public static bool Blocked;

    sbyte horizontal;
    bool jump;
    bool crouch;
    bool lightAttack;
    bool heavyAttack;
    bool guard;
    bool dash;
    bool counter;
    bool ultimate;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null)
        {
            return;
        }

        bool menu = Blocked || IsCursorFree();
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Blocked = !Blocked;
        }

        SetCursor(true);
        if (menu || Blocked)
        {
            horizontal = 0;
            crouch = false;
            guard = false;
            return;
        }

        int x = 0;
        if (keyboard.aKey.isPressed) x -= 1;
        if (keyboard.dKey.isPressed) x += 1;
        horizontal = (sbyte)Mathf.Clamp(x, -1, 1);

        crouch = keyboard.sKey.isPressed;
        guard = mouse.rightButton.isPressed;
        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) jump = true;
        if (mouse.leftButton.wasPressedThisFrame) lightAttack = true;
        if (keyboard.eKey.wasPressedThisFrame) heavyAttack = true;
        if (keyboard.leftShiftKey.wasPressedThisFrame) dash = true;
        if (keyboard.fKey.wasPressedThisFrame) counter = true;
        if (keyboard.qKey.wasPressedThisFrame) ultimate = true;
    }

    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (runner.ProvideInput == false)
        {
            return;
        }

        var data = new CombatNetworkInput();
        if (Blocked == false)
        {
            data.Horizontal = horizontal;
            data.Buttons.Set(ActionButton.Jump, jump);
            data.Buttons.Set(ActionButton.Crouch, crouch);
            data.Buttons.Set(ActionButton.Attack, lightAttack);
            data.Buttons.Set(ActionButton.Heavy, heavyAttack);
            data.Buttons.Set(ActionButton.Guard, guard);
            data.Buttons.Set(ActionButton.Dash, dash);
            data.Buttons.Set(ActionButton.Counter, counter);
            data.Buttons.Set(ActionButton.Ultimate, ultimate);
        }

        input.Set(data);
        jump = false;
        lightAttack = false;
        heavyAttack = false;
        dash = false;
        counter = false;
        ultimate = false;
    }

    static bool IsCursorFree()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneNames.Play)
        {
            return true;
        }

        MatchStateManager match = MatchStateManager.Active;
        if (match == null)
        {
            return false;
        }

        return match.Phase == MatchPhase.Results;
    }

    static void SetCursor(bool free)
    {
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }
}
