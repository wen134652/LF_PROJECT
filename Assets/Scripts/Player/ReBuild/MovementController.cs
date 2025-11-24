using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("如果未分配配置数据，将使用以下默认值")]
    [Header("Move (默认值)")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float accel = 60f;
    [SerializeField] private float decel = 80f;

    [Header("Crouch Move (默认值)")]
    [SerializeField] private float crouchMoveSpeed = 4f;
    [SerializeField] private float crouchAccel = 30f;
    [SerializeField] private float crouchDecel = 40f;

    [Header("Dash Move (默认值)")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashAccel = 200f;
    [SerializeField] private float dashTime = 0.15f;
    [SerializeField] private float dashCooldown = 0.40f;

    [Header("Jump (默认值)")]
    [SerializeField] private float jumpVelocity = 12f;
    [Tooltip("土狼时间（秒）：玩家在脱离平台一小段时间之内，仍然可以执行跳跃操作。如果玩家因为非跳跃行为刚刚离开地面进入下落状态后的一小段时间内按下跳跃键，则视角色依然在地面上，给玩家跳跃留出更长的容错时间")]
    [SerializeField] private float coyoteTime = 0.10f;
    [Tooltip("跳跃输入缓冲时间（秒）：提前按下跳跃键的缓冲时间，允许玩家在落地前提前按下跳跃键")]
    [SerializeField] private float jumpBuffer = 0.10f;
    [SerializeField] private float jumpHoldTime = 0.2f;

    [Header("Gravity & Fall (默认值)")]
    [SerializeField] private float fallGravityScale = 2.2f;
    [SerializeField] private float earlyReleaseGravityScale = 2.8f;
    [SerializeField] private float maxFallSpeed = -30f;

    // 配置数据引用
    private PlayerConfigData configData;

    [Header("Debug Info (Read Only)")]
    [ReadOnly, SerializeField, Tooltip("当前横向速度（只读）")]
    private float displayHorizontalVelocity = 0f;
    [ReadOnly, SerializeField, Tooltip("当前纵向速度（只读）")]
    private float displayVerticalVelocity = 0f;

    private Rigidbody2D rb;
    private GroundChecker groundChecker;
    
    // 当前水平速度（用于基于位置的移动）
    private float currentHorizontalVelocity = 0f;
    // 当前垂直速度（用于基于位置的移动）
    private float currentVerticalVelocity = 0f;
    
    private float lastGroundTime;
    private float lastJumpPress;
    private float faceDir = 1f;
    private bool dashing;
    private float dashTimer;
    private float dashCdTimer;
    private bool jumpHeld;
    private bool isCrouching = false;
    
    // 跳跃相关状态
    private float jumpHoldTimer = 0f;
    private bool isJumping = false;

    public float FaceDir => faceDir;
    public bool IsDashing => dashing;
    public bool CanMove => !dashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundChecker = GetComponent<GroundChecker>();
        
        // 配置 Rigidbody2D 以支持基于位置的移动
        // 设置为 Kinematic 或确保不会干扰位置移动
        if (rb != null)
        {
            // 如果使用 Dynamic，需要冻结位置约束或设置为 Kinematic
            // 这里保持 Dynamic 以便碰撞检测，但通过直接设置位置来移动
            rb.gravityScale = 0f; // 禁用物理重力，我们手动处理
            rb.drag = 0f; // 禁用阻力
        }
    }

    /// <summary>
    /// 从配置数据初始化参数
    /// </summary>
    public void InitializeFromConfig(PlayerConfigData config)
    {
        if (config == null) return;

        configData = config;

        // 从配置数据读取移动参数
        moveSpeed = config.moveSpeed;
        accel = config.accel;
        decel = config.decel;

        // 从配置数据读取蹲下移动参数
        crouchMoveSpeed = config.crouchMoveSpeed;
        crouchAccel = config.crouchAccel;
        crouchDecel = config.crouchDecel;

        // 从配置数据读取冲刺移动参数
        dashSpeed = config.dashSpeed;
        dashAccel = config.dashAccel;
        dashTime = config.dashTime;
        dashCooldown = config.dashCooldown;

        // 从配置数据读取跳跃参数
        jumpVelocity = config.jumpVelocity;
        coyoteTime = config.coyoteTime;
        jumpBuffer = config.jumpBuffer;
        jumpHoldTime = config.jumpHoldTime;

        // 从配置数据读取重力和下落参数
        fallGravityScale = config.fallGravityScale;
        earlyReleaseGravityScale = config.earlyReleaseGravityScale;
        maxFallSpeed = config.maxFallSpeed;
    }

    public void SetJumpHeld(bool held)
    {
        jumpHeld = held;
    }

    public void SetCrouching(bool crouching)
    {
        isCrouching = crouching;
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
            // 冲刺时立即设置速度方向
            currentHorizontalVelocity = faceDir * dashSpeed;
        }
    }

    public void UpdateMovement(float deltaTime)
    {
        // 更新土狼时间（Coyote Time）：如果在地面上，重置土狼时间窗口
        // 如果不在空中，递减土狼时间，允许玩家在离开地面后的一小段时间内仍可跳跃
        if (groundChecker != null && groundChecker.IsGrounded())
        {
            lastGroundTime = coyoteTime;
        }
        else
        {
            lastGroundTime -= deltaTime;
        }

        // 更新跳跃输入缓冲时间
        if (lastJumpPress > 0f)
        {
            lastJumpPress -= deltaTime;
        }

        // 更新冲刺冷却时间
        if (dashCdTimer > 0f)
        {
            dashCdTimer -= deltaTime;
        }
        
        // 更新跳跃保持计时器（用于限制最长跳跃键输入时间）
        if (isJumping && jumpHoldTimer > 0f)
        {
            jumpHoldTimer -= deltaTime;
        }
    }

    public void FixedUpdateMovement(Vector2 moveInput)
    {
        float deltaTime = Time.fixedDeltaTime;
        float inputX = moveInput.x;

        // 更新显示的速度值（用于Inspector显示）
        displayHorizontalVelocity = currentHorizontalVelocity;
        displayVerticalVelocity = currentVerticalVelocity;

        // 处理冲刺移动
        if (dashing)
        {
            dashTimer -= deltaTime;
            if (dashTimer <= 0f)
            {
                dashing = false;
                currentHorizontalVelocity = 0f;
            }
            else
            {
                // 冲刺时保持固定速度
                float targetSpeed = faceDir * dashSpeed;
                float diff = targetSpeed - currentHorizontalVelocity;
                float accelAmount = dashAccel * deltaTime;
                currentHorizontalVelocity += Mathf.Clamp(diff, -accelAmount, accelAmount);
                
                // 计算水平位移并应用
                float dashDisplacementX = currentHorizontalVelocity * deltaTime;
                
                // 处理垂直移动（跳跃和重力）
                UpdateVerticalMovement(deltaTime);
                float dashDisplacementY = currentVerticalVelocity * deltaTime;
                
                transform.position += new Vector3(dashDisplacementX, dashDisplacementY, 0f);
                
                // 确保 Rigidbody2D 的 velocity 被清零
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }
            return;
        }

        // 常规移动和蹲下移动
        float maxSpeed;
        float acceleration;
        float deceleration;

        if (isCrouching)
        {
            maxSpeed = crouchMoveSpeed;
            acceleration = crouchAccel;
            deceleration = crouchDecel;
        }
        else
        {
            maxSpeed = moveSpeed;
            acceleration = accel;
            deceleration = decel;
        }

        // 计算目标速度
        float targetVelocity = inputX * maxSpeed;
        
        // 根据输入决定使用加速度还是减速度
        float a = Mathf.Abs(targetVelocity) > 0.01f ? acceleration : deceleration;
        
        // 计算速度差
        float velocityDiff = targetVelocity - currentHorizontalVelocity;
        
        // 应用加速度限制
        float velocityChange = Mathf.Clamp(velocityDiff, -a * deltaTime, a * deltaTime);
        currentHorizontalVelocity += velocityChange;

        // 计算水平位移
        float displacementX = currentHorizontalVelocity * deltaTime;
        
        // 处理垂直移动（跳跃和重力）
        UpdateVerticalMovement(deltaTime);
        float displacementY = currentVerticalVelocity * deltaTime;

        // 应用位移
        transform.position += new Vector3(displacementX, displacementY, 0f);
        
        // 确保 Rigidbody2D 的 velocity 被清零，防止物理系统干扰
        if (rb != null)
            {
            rb.velocity = Vector2.zero;
        }

        // 更新面向方向
        if (Mathf.Abs(inputX) > 0.05f)
        {
            faceDir = Mathf.Sign(inputX);
            transform.localScale = new Vector3(faceDir, 1f, 1f);
        }
    }

    /// <summary>
    /// 更新垂直移动（解耦的跳跃和下落逻辑）
    /// </summary>
    private void UpdateVerticalMovement(float deltaTime)
    {
        // 检测地面状态
        bool isGrounded = groundChecker != null && groundChecker.IsGrounded();
        
        // ========== 地面状态处理 ==========
        if (isGrounded)
        {
            // 处理跳跃输入（在地面上时可以开始跳跃）
            if (lastJumpPress > 0f && lastGroundTime > 0f && !isJumping)
            {
                // 开始跳跃：设置向上的固定速度
                StartJump();
            }
            else
            {
                // 在地面上且没有跳跃输入：停止Y轴一切速度，重置跳跃状态
                StopVerticalMovement();
                return;
            }
        }
        
        // ========== 空中状态处理 ==========
        // 只有在不在地面上时，才进行垂直速度计算
        
        // 土狼时间跳跃检查：如果玩家刚离开地面且在土狼时间内，允许跳跃
        // 正常情况下，玩家在下落状态下无法使用跳跃键
        // 但是如果玩家因为非跳跃行为刚刚离开地面进入下落状态后的一小段时间（土狼时间）内按下跳跃键
        // 则视角色依然在地面上，给玩家跳跃留出更长的容错时间
        if (!isJumping && lastJumpPress > 0f && lastGroundTime > 0f)
        {
            // 在土狼时间内，视角色依然在地面上，允许跳跃
            StartJump();
        }
        
        // 先处理跳跃逻辑（如果正在跳跃且速度为正）
        if (isJumping && currentVerticalVelocity > 0f)
        {
            UpdateJumpLogic(deltaTime);
        }
        
        // 然后处理下落逻辑（如果不在跳跃状态或速度已转为向下）
        // 注意：提前松开跳跃键后，如果速度已转为向下，会退出跳跃状态，交由下落逻辑处理
        if (!isJumping || currentVerticalVelocity <= 0f)
        {
            UpdateFallLogic(deltaTime);
        }
    }
    
    /// <summary>
    /// 开始跳跃：将纵向速度设置为正方向的固定速度
    /// </summary>
    private void StartJump()
    {
        currentVerticalVelocity = jumpVelocity;
        isJumping = true;
        jumpHoldTimer = jumpHoldTime;
        lastJumpPress = 0f;
        lastGroundTime = 0f;
    }
    
    /// <summary>
    /// 停止垂直移动：在地面上时停止Y轴一切速度，重置跳跃状态
    /// </summary>
    private void StopVerticalMovement()
    {
        currentVerticalVelocity = 0f;
        if (isJumping)
        {
            isJumping = false;
            jumpHoldTimer = 0f;
        }
    }
    
    /// <summary>
    /// 更新跳跃逻辑：处理跳跃键按住和松开的状态
    /// </summary>
    private void UpdateJumpLogic(float deltaTime)
        {
        // 提前松开跳跃键（不在按住状态但时间还没结束）- 优先检查
        if (!jumpHeld && jumpHoldTimer > 0f)
        {
            // 应用基础重力 + 额外配置重力 earlyReleaseGravityScale
            float baseGravity = Physics2D.gravity.y * fallGravityScale;
            float earlyReleaseGravity = Physics2D.gravity.y * earlyReleaseGravityScale;
            float totalAcceleration = baseGravity + earlyReleaseGravity;
            
            currentVerticalVelocity += totalAcceleration * deltaTime;
            
            // 确保不会超过最大下落速度
            if (currentVerticalVelocity < maxFallSpeed)
            {
                currentVerticalVelocity = maxFallSpeed;
            }
            
            // 如果速度已转为向下，退出跳跃状态，交由下落逻辑处理
            if (currentVerticalVelocity <= 0f)
            {
                isJumping = false;
                jumpHoldTimer = 0f;
            }
            return; // 提前松开时，直接返回，不检查其他条件
        }
        
        // 如果持续按住跳跃键且在最长跳跃键输入时间内
        if (jumpHeld && jumpHoldTimer > 0f)
        {
            // 维持初始的向上速度（不应用重力，保持匀速上升）
            // 速度保持不变，不进行任何加速度计算
            return;
        }
        
        // 如果时间结束（jumpHoldTimer <= 0f），无论是否还在按住
        // 退出跳跃状态，交由下落逻辑处理（只应用基础重力，不应用 earlyReleaseGravityScale）
        if (jumpHoldTimer <= 0f)
        {
            isJumping = false;
            jumpHoldTimer = 0f;
            // 退出跳跃状态后，UpdateFallLogic 会处理基础重力
        }
    }
    
    /// <summary>
    /// 更新下落逻辑：处理重力加速度和最大下落速度限制
    /// </summary>
    private void UpdateFallLogic(float deltaTime)
    {
        // 如果在地面上，不进行下落加速度计算
        bool isGrounded = groundChecker != null && groundChecker.IsGrounded();
        if (isGrounded)
        {
            return;
        }
        
        // 如果在空中，受到向下的重力加速度影响
        float gravityAcceleration = Physics2D.gravity.y * fallGravityScale;
        currentVerticalVelocity += gravityAcceleration * deltaTime;
        
        // 限制最大下落速度：到达最大下落速度时保持匀速
        if (currentVerticalVelocity < maxFallSpeed)
        {
            currentVerticalVelocity = maxFallSpeed;
        }
    }

    public void StopHorizontalMovement()
    {
        currentHorizontalVelocity = 0f;
        // 不再需要设置 rb.velocity，因为移动完全由位置控制
    }

    public void SetFaceDir(float dir)
    {
        faceDir = dir;
        transform.localScale = new Vector3(faceDir, 1f, 1f);
    }
}