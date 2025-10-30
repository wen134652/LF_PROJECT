using UnityEngine;
using UnityEngine.InputSystem;
//用于绑定动作键盘、手柄输入
[DefaultExecutionOrder(-10)]
public class MyPlayerInput : MonoBehaviour
{
    public MyCharacterController controller;

    private InputActionMap map;
    private InputAction move;
    private InputAction jump;
    private InputAction leftHand;
    private InputAction rightHand;
    private InputAction aimLeft;
    private InputAction aimRight;
    private InputAction throwLeft;
    private InputAction throwRight;
    private InputAction aimStick;
    private InputAction openInventory;
    private InputAction interAction;//与地图交互
    private InputAction quickNoteTap;
    private InputAction noteHold;

    private bool handBusy = false;
    public float handLockDuration = 0.15f;

    private Vector2 aimDir = Vector2.right;

    bool bagOpening = false;

    void Awake()
    {
        if (!controller) controller = GetComponent<MyCharacterController>();

        map = new InputActionMap("Player");

        //左右移动
        {
            move = map.AddAction(name: "Move", type: InputActionType.Value);
            move.expectedControlType = "Vector2";
            var wasd = move.AddCompositeBinding("2DVector");
            wasd.With("up", "<Keyboard>/w");
            wasd.With("down", "<Keyboard>/s");
            wasd.With("left", "<Keyboard>/a");
            wasd.With("right", "<Keyboard>/d");

            //绑定手柄左摇杆     
            move.AddBinding("<Gamepad>/leftStick");

            // 绑定move
            move.performed += controller.OnMove;
            move.canceled += controller.OnMove;
        }

        //起跳
        {
            jump = map.AddAction(name: "Jump", type: InputActionType.Button);
            jump.AddBinding("<Keyboard>/space");

            //键盘绑定手柄
            jump.AddBinding("<Gamepad>/buttonSouth");

            //绑定jump
            jump.performed += controller.OnJump;
        }

        //左右手使用
        {
            leftHand = map.AddAction("UseLeftHand", type: InputActionType.Button);
            rightHand = map.AddAction("UseRightHand", type: InputActionType.Button);
            leftHand.AddBinding("<Keyboard>/q");//键盘左右手攻击
            rightHand.AddBinding("<Keyboard>/e");
            leftHand.AddBinding("<Gamepad>/leftShoulder");   // LB
            rightHand.AddBinding("<Gamepad>/rightShoulder");  // RB
            leftHand.performed += ctx => 
            {
                if (!handBusy)
                {
                    handBusy = true;
                    controller.OnUseLeftHand(ctx);
                }
            };
            rightHand.performed += ctx =>
            {
                if (!handBusy)
                {
                    handBusy = true;
                    controller.OnUseRightHand(ctx);
                }
            };
        }
        //左右手丢出
        {
            //绑定右摇杆
            {
                aimStick = map.AddAction("Aim", InputActionType.Value);
                aimStick.expectedControlType = "Vector2";
                aimStick.AddBinding("<Gamepad>/rightStick");
                aimStick.performed += ctx =>
                {
                    Vector2 v = ctx.ReadValue<Vector2>();
                    if (v.sqrMagnitude > 0.0001f)
                        aimDir = v.normalized;
                    controller.UpdateAimDirection(aimDir);
                };
                aimStick.canceled += ctx =>
                {
                    Vector2 v = ctx.ReadValue<Vector2>();
                    if (v.sqrMagnitude > 0.0001f)
                        aimDir = v.normalized;
                    controller.UpdateAimDirection(aimDir);
                };
                
            }
            //左右丢出
            {
                throwLeft = map.AddAction("ThrowLeft", InputActionType.Button);
                throwLeft.AddBinding("<Gamepad>/leftTrigger");
                throwLeft.started += ctx =>
                  {
                      if (!handBusy)
                      {
                          handBusy = true;
                          controller.BeginAim(true);
                      }
                  };
                throwRight = map.AddAction("ThrowRight", InputActionType.Button);
                throwRight.AddBinding("<Gamepad>/rightTrigger");
                throwRight.started += ctx =>
                 {
                     if (!handBusy)
                     {
                         handBusy = true;
                         controller.BeginAim(false);
                    }
                 };
                throwLeft.canceled += ctx =>
                 {
                     controller.ReleaseAim(ctx);
                     handBusy = false;
                 };
                throwRight.canceled += ctx =>
                {
                    controller.ReleaseAim(ctx);
                    handBusy = false;
                };

            }
        }


        // ---- 背包 (Y / Tab) ----
        {
            openInventory = map.AddAction("OpenInventory", InputActionType.Button);
            openInventory.AddBinding("<Keyboard>/tab");
            openInventory.AddBinding("<Gamepad>/buttonNorth");
            openInventory.performed += ctx =>
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
             };
        }


        // ---- 交易/特殊交互 (B / E) ----
        {
            interAction = map.AddAction("TradeOrSpecial", InputActionType.Button);
            interAction.AddBinding("<Keyboard>/e");
            interAction.AddBinding("<Gamepad>/buttonEast");
            interAction.performed += ctx =>
            {
                controller.OnSpecial(ctx);
            };
        }

        // ---- 记事 (X 短按 / 长按0.5s) ----
        {
            quickNoteTap = map.AddAction("QuickNoteTap", InputActionType.Button);
            quickNoteTap.AddBinding("<Gamepad>/buttonWest");
            quickNoteTap.AddBinding("<Keyboard>/n");
            quickNoteTap.performed += ctx =>
            {
                controller.OnQuickNoteTap(ctx);
            };
        }
        
        {
            noteHold = map.AddAction("NoteHold", InputActionType.Button);
            var holdPad = noteHold.AddBinding("<Gamepad>/buttonWest");
            holdPad.WithInteractions ("hold(duration=0.5)");
            var holdKey = noteHold.AddBinding("<Keyboard>/n");
            holdKey.WithInteractions( "hold(duration=0.5)");
            noteHold.performed += ctx =>
             {
                 controller.OnNoteHold(ctx);
             };
        }
    }

    void OnEnable() => map.Enable();
    void OnDisable() => map.Disable();

    void OnDestroy()
    {
        move.performed -= controller.OnMove;
        move.canceled -= controller.OnMove;
        map.Dispose();
    }
}
