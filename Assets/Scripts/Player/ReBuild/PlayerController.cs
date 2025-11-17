using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(ClimbController))]
[RequireComponent(typeof(GroundChecker))]
public class PlayerController : MonoBehaviour
{
    private bool controlLocked = false;

    [Header("Inventory")]
    public GameObject inventoryUI;

    [Header("Components")]
    private MovementController movementController;
    private ClimbController climbController;
    private ThrowController throwController;
    private InputHandler inputHandler;
    private GroundChecker groundChecker;

    private bool inventoryOpen = false;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        climbController = GetComponent<ClimbController>();
        throwController = GetComponent<ThrowController>();
        groundChecker = GetComponent<GroundChecker>();

        inputHandler = GetComponent<InputHandler>();
        if (inputHandler == null)
        {
            inputHandler = gameObject.AddComponent<InputHandler>();
        }
    }

    private void Start()
    {
        // 初始化时同步投掷方向，避免开局面朝方向不一致
        if (throwController != null && movementController != null)
        {
            throwController.FaceDir = movementController.FaceDir;
        }
    }

    private void Update()
    {
        if (controlLocked || inventoryOpen)
        {
            inputHandler?.ClearMoveInput();
            return;
        }


        // 常规移动：非攀爬状态下由MovementController更新
        if (movementController != null && !climbController.IsClimbing)
        {
            movementController.UpdateMovement(Time.deltaTime);
        }

        // 攀爬状态：交给ClimbController处理
        if (climbController.IsClimbing)
        {
            climbController.UpdateClimb(inputHandler.MoveAxis, movementController.FaceDir);
        }

        // 让投掷控制器保持与最新面向一致
        if (throwController != null && movementController != null)
        {
            throwController.FaceDir = movementController.FaceDir;
        }

    }

    private void FixedUpdate()
    {
        if (controlLocked || inventoryOpen)
        {
            return;
        }


        // 攀爬时不参与地面运动，直接交由攀爬控制器处理
        if (climbController.IsClimbing)
        {
            climbController.FixedUpdateClimb(inputHandler.MoveAxis);
            return;
        }

        // 常规地面/空中运动
        if (movementController != null)
        {

            // 广播一次性输入
            if (inputHandler.JumpPressed)
            {
                movementController.TryJump();
            }

            if (inputHandler.DashPressed)
            {
                movementController.TryDash();
            }

            movementController.SetJumpHeld(inputHandler.JumpHeld);
            movementController.FixedUpdateMovement(inputHandler.MoveAxis);
        }

        // 物理帧结束后清理本帧输入状态
        inputHandler?.ResetFrameInputs();
    }


    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (inventoryOpen)
        {
            inputHandler?.ClearMoveInput();
            return;
        }
        inputHandler?.OnMove(ctx);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (inventoryOpen) return;


        if (ctx.performed && climbController.IsClimbing)
        {
            climbController.ExitClimb();
        }

        inputHandler?.OnJump(ctx);
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (inventoryOpen) return;
        inputHandler?.OnDash(ctx);
    }

    public void OnUseLeftHand(InputAction.CallbackContext ctx)
    {
        inputHandler?.OnUseLeftHand(ctx);
    }

    public void OnUseRightHand(InputAction.CallbackContext ctx)
    {
        inputHandler?.OnUseRightHand(ctx);
    }

    public void OnOpenInventory(InputAction.CallbackContext ctx)
    {
        inventoryOpen = true;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
        SetControlLocked(true);
    }

    public void OnCloseInventory(InputAction.CallbackContext ctx)
    {
        inventoryOpen = false;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        SetControlLocked(false);
    }

    public void OnSpecial(InputAction.CallbackContext ctx) { }

    public void OnQuickNoteTap(InputAction.CallbackContext ctx) { }

    public void OnNoteHold(InputAction.CallbackContext ctx) { }


    public void BeginAim(bool useLeftHand)
    {
        if (throwController != null)
        {
            throwController.BeginAim(useLeftHand);
        }
    }

    public void UpdateAimDirection(Vector2 dirFromStick)
    {
        if (throwController != null)
        {
            throwController.UpdateAimDirection(dirFromStick);
        }
    }

    public void ReleaseAim(InputAction.CallbackContext ctx)
    {
        if (throwController != null)
        {
            throwController.ReleaseAim();
        }
    }


    public void EnterClimb(Transform anchor, Vector2 upDir, Vector2 rightDir, MonoBehaviour source)
    {
        if (climbController != null)
        {
            climbController.EnterClimb(anchor, upDir, rightDir, source);
        }
    }

    public void ExitClimb()
    {
        if (climbController != null)
        {
            climbController.ExitClimb();
        }
    }


    public void SetControlLocked(bool locked)
    {
        controlLocked = locked;

        if (locked)
        {
            if (movementController != null)
            {
                movementController.StopHorizontalMovement();
            }
            inputHandler?.ClearMoveInput();
        }
    }


    public bool IsClimbing => climbController != null && climbController.IsClimbing;
    public bool IsDashing => movementController != null && movementController.IsDashing;
    public bool IsAiming => throwController != null && throwController.IsAiming;
}