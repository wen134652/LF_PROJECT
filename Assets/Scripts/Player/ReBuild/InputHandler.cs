using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private Vector2 moveAxis;
    private bool jumpPressed;
    private bool dashPressed;
    private bool jumpHeld;
    private bool leftHand;
    private bool rightHand;
    private Vector2 aimDirection;

    public Vector2 MoveAxis => moveAxis;
    public bool JumpPressed => jumpPressed;
    public bool DashPressed => dashPressed;
    public bool JumpHeld => jumpHeld;
    public bool LeftHand => leftHand;
    public bool RightHand => rightHand;
    public Vector2 AimDirection => aimDirection;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveAxis = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // 处理按下状态
        if (ctx.started || ctx.performed)
        {
            jumpPressed = true;
            jumpHeld = true;
        }
        // 处理松开状态
        if (ctx.canceled)
        {
            jumpHeld = false;
        }
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            dashPressed = true;
        }
    }

    public void OnUseLeftHand(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            leftHand = true;
        }
    }

    public void OnUseRightHand(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            rightHand = true;
        }
    }

    public void OnAimDirection(InputAction.CallbackContext ctx)
    {
        aimDirection = ctx.ReadValue<Vector2>();
    }

    // 每帧重置单次触发输入
    public void ResetFrameInputs()
    {
        jumpPressed = false;
        dashPressed = false;
        leftHand = false;
        rightHand = false;
    }

    public void ClearMoveInput()
    {
        moveAxis = Vector2.zero;
    }
}