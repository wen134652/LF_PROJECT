using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 按键绑定配置 - 用于在Inspector中配置键盘和手柄按键
/// </summary>
[System.Serializable]
public class KeyBinding
{
    [Tooltip("键盘按键路径 (例如: <Keyboard>/space)")]
    public string keyboardBinding = "";
    
    [Tooltip("手柄按键路径 (例如: <Gamepad>/buttonSouth)")]
    public string gamepadBinding = "";
}

/// <summary>
/// 2D向量移动绑定配置 - 用于移动输入（WASD/方向键）
/// </summary>
[System.Serializable]
public class Move2DBinding
{
    [Tooltip("上方向键 (例如: <Keyboard>/w)")]
    public string keyboardUp = "<Keyboard>/w";
    
    [Tooltip("下方向键 (例如: <Keyboard>/s)")]
    public string keyboardDown = "<Keyboard>/s";
    
    [Tooltip("左方向键 (例如: <Keyboard>/a)")]
    public string keyboardLeft = "<Keyboard>/a";
    
    [Tooltip("右方向键 (例如: <Keyboard>/d)")]
    public string keyboardRight = "<Keyboard>/d";
    
    [Tooltip("手柄左摇杆 (例如: <Gamepad>/leftStick)")]
    public string gamepadStick = "<Gamepad>/leftStick";
}

/// <summary>
/// 玩家输入处理脚本 - 支持在Inspector中配置按键绑定
/// </summary>
[DefaultExecutionOrder(-10)]
public class MyPlayerInput : MonoBehaviour
{
    [Header("控制器引用")]
    [Tooltip("玩家控制器组件")]
    public PlayerController controller;

    [Header("移动输入配置")]
    [Tooltip("2D移动按键绑定（WASD/方向键 + 手柄摇杆）")]
    public Move2DBinding moveBinding = new Move2DBinding();

    [Header("基础动作配置")]
    [Tooltip("跳跃按键绑定")]
    public KeyBinding jumpBinding = new KeyBinding { keyboardBinding = "<Keyboard>/space", gamepadBinding = "<Gamepad>/buttonSouth" };

    [Tooltip("冲刺按键绑定")]
    public KeyBinding dashBingding = new KeyBinding { keyboardBinding = "<Keyboard/leftShift>", gamepadBinding = "<Gamepad>/bottonWest" };

    [Header("手部动作配置")]
    [Tooltip("左手使用按键绑定")]
    public KeyBinding leftHandBinding = new KeyBinding { keyboardBinding = "<Keyboard>/q", gamepadBinding = "<Gamepad>/leftShoulder" };
    
    [Header("投掷和瞄准配置")]
    [Tooltip("左手投掷按键绑定")]
    public KeyBinding throwLeftBinding = new KeyBinding { keyboardBinding = "<Keyboard>/o", gamepadBinding = "<Gamepad>/leftTrigger" };
    
    [Tooltip("瞄准摇杆（手柄右摇杆）")]
    public string aimStickBinding = "<Gamepad>/rightStick";
    
    [Tooltip("瞄准摇杆死区最小值")]
    [Range(0f, 1f)]
    public float aimDeadzoneMin = 0.25f;

    [Header("系统功能配置")]
    [Tooltip("打开背包按键绑定")]
    public KeyBinding openInventoryBinding = new KeyBinding { keyboardBinding = "<Keyboard>/tab", gamepadBinding = "<Gamepad>/buttonNorth" };
    
    [Tooltip("交互按键绑定")]
    public KeyBinding interactBinding = new KeyBinding { keyboardBinding = "<Keyboard>/f", gamepadBinding = "<Gamepad>/dpad/up" };

    [Tooltip("拾取按键绑定")]
    public KeyBinding pickUpBinding = new KeyBinding { keyboardBinding = "<Keyboard>/e", gamepadBinding = "<Gamepad>/buttonEast" };

    [Header("其他设置")]
    [Tooltip("手部动作锁定持续时间（秒）")]
    public float handLockDuration = 0.15f;

    // InputActionMap和InputAction引用
    private InputActionMap map;
    
    // 基础行动
    private InputAction move;
    private InputAction jump;
    private InputAction dash;

    // 使用/投掷/瞄准     
    private InputAction leftHand;
    private InputAction throwLeft;
    private InputAction aimStick;

    // 系统与交互
    private InputAction openInventory;
    private InputAction interAction;
    private InputAction pickUp;

    // 订阅解绑集中管理
    private readonly List<System.Action> unbindActions = new List<System.Action>();

    // 手部忙碌锁
    private bool handBusy = false;

    // 右摇杆瞄准方向，默认向右
    private Vector2 aimDir = Vector2.right;

    // 背包开关
    private bool bagOpening = false;

    private void Awake()
    {
        if (!controller) 
            controller = GetComponent<PlayerController>();

        if (controller == null)
        {
            Debug.LogError("MyPlayerInput: 未找到PlayerController组件！", this);
            return;
        }

        map = new InputActionMap("Player");

        SetupMove();
        SetupJump();
        SetupUseHands();
        SetupThrowAndAim();
        SetupInventory();
        SetupInteraction();
        SetupPickUp();
    }

    private void OnEnable() 
    {
        if (map != null)
            map.Enable();
    }
    
    private void OnDisable() 
    {
        if (map != null)
            map.Disable();
    }

    private void OnDestroy()
    {
        // 统一解绑，避免事件泄漏
        for (int i = unbindActions.Count - 1; i >= 0; i--)
        {
            try { unbindActions[i]?.Invoke(); }
            catch { /* 忽略异常，尽量释放 */ }
        }
        unbindActions.Clear();

        map?.Dispose();
    }

    #region Setup Methods

    /// <summary>
    /// 设置移动输入
    /// </summary>
    private void SetupMove()
    {
        move = map.AddAction(name: "Move", type: InputActionType.Value);
        move.expectedControlType = "Vector2";

        // 设置键盘2D向量绑定
        if (!string.IsNullOrEmpty(moveBinding.keyboardUp) ||
            !string.IsNullOrEmpty(moveBinding.keyboardDown) ||
            !string.IsNullOrEmpty(moveBinding.keyboardLeft) ||
            !string.IsNullOrEmpty(moveBinding.keyboardRight))
        {
            var wasd = move.AddCompositeBinding("2DVector");
            
            if (!string.IsNullOrEmpty(moveBinding.keyboardUp))
                wasd.With("up", moveBinding.keyboardUp);
            if (!string.IsNullOrEmpty(moveBinding.keyboardDown))
                wasd.With("down", moveBinding.keyboardDown);
            if (!string.IsNullOrEmpty(moveBinding.keyboardLeft))
                wasd.With("left", moveBinding.keyboardLeft);
            if (!string.IsNullOrEmpty(moveBinding.keyboardRight))
                wasd.With("right", moveBinding.keyboardRight);
        }

        // 设置手柄摇杆绑定
        if (!string.IsNullOrEmpty(moveBinding.gamepadStick))
        {
            move.AddBinding(moveBinding.gamepadStick);
        }

        System.Action<InputAction.CallbackContext> moveHandler = controller.OnMove;
        move.performed += moveHandler;
        move.canceled += moveHandler;

        unbindActions.Add(() => { move.performed -= moveHandler; move.canceled -= moveHandler; });
    }

    /// <summary>
    /// 设置跳跃输入
    /// </summary>
    private void SetupJump()
    {
        jump = map.AddAction(name: "Jump", type: InputActionType.Button);
        
        AddBindingToAction(jump, jumpBinding);

        System.Action<InputAction.CallbackContext> jumpHandler = controller.OnJump;
        // 订阅所有事件：started, performed, canceled，确保能正确检测按下和松开
        jump.started += jumpHandler;
        jump.performed += jumpHandler;
        jump.canceled += jumpHandler;

        unbindActions.Add(() => 
        { 
            jump.started -= jumpHandler; 
            jump.performed -= jumpHandler; 
            jump.canceled -= jumpHandler; 
        });
    }

    /// <summary>
    /// 设置手部使用输入
    /// </summary>
    private void SetupUseHands()
    {
        leftHand = map.AddAction("UseLeftHand", type: InputActionType.Button);

        AddBindingToAction(leftHand, leftHandBinding);

        System.Action<InputAction.CallbackContext> leftHandler = OnLeftHandPerformed;

        leftHand.performed += leftHandler;

        unbindActions.Add(() => leftHand.performed -= leftHandler);
    }

    /// <summary>
    /// 设置投掷和瞄准输入
    /// </summary>
    private void SetupThrowAndAim()
    {
        // 右摇杆瞄准，加入 deadzone 处理
        aimStick = map.AddAction("Aim", InputActionType.Value);
        aimStick.expectedControlType = "Vector2";
        
        if (!string.IsNullOrEmpty(aimStickBinding))
        {
            var binding = aimStick.AddBinding(aimStickBinding);
            binding.WithProcessor($"stickDeadzone(min={aimDeadzoneMin})");
        }

        System.Action<InputAction.CallbackContext> aimChanged = OnAimChanged;
        aimStick.performed += aimChanged;
        aimStick.canceled += aimChanged;

        unbindActions.Add(() => { aimStick.performed -= aimChanged; aimStick.canceled -= aimChanged; });

        // 左右投掷
        throwLeft = map.AddAction("ThrowLeft", InputActionType.Button);
        AddBindingToAction(throwLeft, throwLeftBinding);

        System.Action<InputAction.CallbackContext> leftStarted = OnThrowLeftStarted;
        System.Action<InputAction.CallbackContext> leftCanceled = OnThrowLeftCanceled;

        throwLeft.started += leftStarted;
        throwLeft.canceled += leftCanceled;

        unbindActions.Add(() => { throwLeft.started -= leftStarted; throwLeft.canceled -= leftCanceled; });
    }

    /// <summary>
    /// 设置背包输入
    /// </summary>
    private void SetupInventory()
    {
        openInventory = map.AddAction("OpenInventory", InputActionType.Button);
        AddBindingToAction(openInventory, openInventoryBinding);

        System.Action<InputAction.CallbackContext> onInv = OnOpenInventoryPerformed;
        openInventory.performed += onInv;

        unbindActions.Add(() => openInventory.performed -= onInv);
    }

    /// <summary>
    /// 设置交互输入
    /// </summary>
    private void SetupInteraction()
    {
        interAction = map.AddAction("TradeOrSpecial", InputActionType.Button);
        AddBindingToAction(interAction, interactBinding);

        System.Action<InputAction.CallbackContext> onInteract = controller.OnInteract;
        interAction.started += onInteract;
        interAction.performed += onInteract;
        interAction.canceled += onInteract;


        unbindActions.Add(() => interAction.performed -= onInteract);
    }

    /// <summary>
    /// 设置记事输入
    /// </summary>
    private void SetupPickUp()
    {
        // 短按
        pickUp = map.AddAction("pickUp", InputActionType.Button);
        AddBindingToAction(pickUp, pickUpBinding);

        System.Action<InputAction.CallbackContext> pickAction = controller.OnPickUp;
        pickUp.started += pickAction;
        pickUp.performed += pickAction;
        pickUp.canceled += pickAction;

        unbindActions.Add(() => pickUp.performed -= pickAction);
    }

    /// <summary>
    /// 将按键绑定添加到InputAction
    /// </summary>
    private void AddBindingToAction(InputAction action, KeyBinding binding)
    {
        if (action == null) return;

        // 添加键盘绑定
        if (!string.IsNullOrEmpty(binding.keyboardBinding))
        {
            action.AddBinding(binding.keyboardBinding);
        }

        // 添加手柄绑定
        if (!string.IsNullOrEmpty(binding.gamepadBinding))
        {
            action.AddBinding(binding.gamepadBinding);
        }
    }

    #endregion

    #region Event Handlers

    private void OnLeftHandPerformed(InputAction.CallbackContext ctx)
    {
        if (handBusy) return;
        handBusy = true;
        controller.OnUseLeftHand(ctx);
        StartCoroutine(HandAutoUnlock(handLockDuration));
    }


    private void OnAimChanged(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();
        if (v.sqrMagnitude > 0.0001f)
            aimDir = v.normalized;

        controller.UpdateAimDirection(aimDir);
    }

    private void OnThrowLeftStarted(InputAction.CallbackContext ctx)
    {
        if (handBusy) return;
        handBusy = true;
        controller.BeginAim(true); // 左手开始瞄准
    }

    private void OnThrowLeftCanceled(InputAction.CallbackContext ctx)
    {
        controller.ReleaseAim(ctx); // 左手释放投掷
        handBusy = false;
    }

    private void OnOpenInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (!bagOpening)
        {
            bagOpening = true;
            controller.OnOpenInventory(ctx);
        }
        else
        {
            bagOpening = false;
            controller.OnCloseInventory(ctx);
        }
    }

    #endregion

    #region Utility Methods

    private IEnumerator HandAutoUnlock(float duration)
    {
        if (duration <= 0f)
        {
            handBusy = false;
            yield break;
        }
        yield return new WaitForSeconds(duration);
        handBusy = false;
    }

    #endregion
}
