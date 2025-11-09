using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 8f;
    public float accel = 60f;
    public float decel = 80f;

    [Header("Jump")]
    public float jumpVelocity = 12f;
    public float coyoteTime = 0.10f;
    public float jumpBuffer = 0.10f;

    [Header("Better Jump")]
    public float fallGravityScale = 2.2f;
    public float lowJumpGravityScale = 2.8f;

    [Header("Dash")]
    public float dashSpeed = 16f;
    public float dashTime = 0.15f;
    public float dashCooldown = 0.40f;

    private Rigidbody2D rb;
    private GroundChecker groundChecker;
    
    private float lastGroundTime;
    private float lastJumpPress;
    private float faceDir = 1f;
    private bool dashing;
    private float dashTimer;
    private float dashCdTimer;
    private bool jumpHeld;

    public float FaceDir => faceDir;
    public bool IsDashing => dashing;
    public bool CanMove => !dashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundChecker = GetComponent<GroundChecker>();
    }

    public void SetJumpHeld(bool held)
    {
        jumpHeld = held;
    }

    public void TryJump()
    {
        lastJumpPress = jumpBuffer;
    }

    public void TryDash()
    {
        if (dashCdTimer <= 0f && !dashing)
        {
            dashing = true;
            dashTimer = dashTime;
            dashCdTimer = dashTime + dashCooldown;
        }
    }

    public void UpdateMovement(float deltaTime)
    {
        if (groundChecker != null && groundChecker.IsGrounded())
        {
            lastGroundTime = coyoteTime;
        }
        else
        {
            lastGroundTime -= deltaTime;
        }

        if (lastJumpPress > 0f)
        {
            lastJumpPress -= deltaTime;
        }

        if (dashCdTimer > 0f)
        {
            dashCdTimer -= deltaTime;
        }
    }

    public void FixedUpdateMovement(Vector2 moveInput)
    {
        if (dashing)
        {
            rb.velocity = new Vector2(faceDir * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                dashing = false;
            }
            return;
        }

        // 移动
        float inputX = moveInput.x;
        float target = inputX * moveSpeed;
        float diff = target - rb.velocity.x;
        float a = Mathf.Abs(target) > 0.01f ? accel : decel;
        float velX = rb.velocity.x + Mathf.Clamp(diff, -a * Time.fixedDeltaTime, a * Time.fixedDeltaTime);

        // 跳跃
        float velY = rb.velocity.y;
        if (lastJumpPress > 0f && lastGroundTime > 0f)
        {
            velY = jumpVelocity;
            lastJumpPress = 0f;
            lastGroundTime = 0f;
        }

        rb.velocity = new Vector2(velX, velY);

        // Better Jump - 动态重力
        if (rb.velocity.y < -0.01f)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityScale - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0.01f)
        {
            if (!jumpHeld)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityScale - 1f) * Time.fixedDeltaTime;
            }
        }

        // 更新面向方向
        if (Mathf.Abs(inputX) > 0.05f)
        {
            faceDir = Mathf.Sign(inputX);
            transform.localScale = new Vector3(faceDir, 1f, 1f);
        }
    }

    public void StopHorizontalMovement()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    public void SetFaceDir(float dir)
    {
        faceDir = dir;
        transform.localScale = new Vector3(faceDir, 1f, 1f);
    }
}