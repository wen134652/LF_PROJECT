using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MyCharacterController : MonoBehaviour
{
    private bool controlLocked = false;

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

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundMask;

    [Header("Hand")]
    public Transform leftHandPoint;
    public Transform rightHandPoint;
    public GameObject throwPrefab;
    public float throwSpeed = 12f;

    [Header("Aim Line")]
    public LineRenderer aimLine;
    public int lineSegments = 20;//瞄准线分多少段
    public float lineTimeStep = 0.05f;
    private Vector2 currentAimDir = Vector2.right;
    private bool isAiming = false;
    private bool leftAiming = false;
    private bool rightAiming = false;

    [Header("Inventory")]
    public GameObject inventoryUI;
    private bool inventoryOpen = false;

    private Vector2 moveAxis;
    private bool jumpPressed, dashPressed;
    private bool leftHand, rightHand;

    //工具方法：用于启动/冻结角色移动
    public void SetControlLocked(bool locked)
    {
        controlLocked = locked;

        if (locked)
        {
            // 清水平速度并强制停输入
            rb.velocity = new Vector2(0f, rb.velocity.y);
            moveAxis = Vector2.zero;
        }
    }
    public void OnMove(InputAction.CallbackContext ctx) 
    {
        if (inventoryOpen)
        {
            moveAxis = Vector2.zero;
            return;
        }
        moveAxis = ctx.ReadValue<Vector2>(); 
    }
    public void OnJump(InputAction.CallbackContext ctx) 
    {
        if (inventoryOpen)
            return;
        if (ctx.performed) jumpPressed = true; 
    }
    public void OnDash(InputAction.CallbackContext ctx) { if (ctx.performed) dashPressed = true; }
    public void OnUseLeftHand(InputAction.CallbackContext ctx) { }
    public void OnUseRightHand(InputAction.CallbackContext ctx) { }
    public void OnOpenInventory(InputAction.CallbackContext ctx) 
    {
        inventoryOpen = true;

        if (inventoryUI != null)
            inventoryUI.SetActive(true);

        SetControlLocked(true);
    }
    public void OnCloseInventory(InputAction.CallbackContext ctx) 
    {
        inventoryOpen = false;

        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        SetControlLocked(false);
    }
    public void OnSpecial(InputAction.CallbackContext ctx) { }
    public void OnQuickNoteTap(InputAction.CallbackContext ctx) { }
    public void OnNoteHold(InputAction.CallbackContext ctx) { }


    // 根据起点+方向+速度，实例化抛掷物并给初速度
    private void ThrowFromHand(Transform handPoint, Vector2 aimDir)
    {
        if (throwPrefab == null || handPoint == null) return;

        // 没有方向就默认向角色面朝方向
        if (aimDir.sqrMagnitude < 0.0001f)
        {
            aimDir = new Vector2(faceDir, 0f);
        }

        GameObject proj = Instantiate(throwPrefab, handPoint.position, Quaternion.identity);

        var projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.velocity = aimDir.normalized * throwSpeed;
        }
    }

    // 画一条抛物线（用 LineRenderer）
    private void DrawAimArc(Vector2 dir)
    {
        if (aimLine == null) return;

        // 起点：优先右手点，其次左手点，最后玩家自己
        Vector3 startPos;
        if (rightAiming && rightHandPoint != null)
        {
            startPos = rightHandPoint.position;
        }
        else if (leftAiming && leftHandPoint != null)
        {
            startPos = leftHandPoint.position;
        }
        else if (rightHandPoint != null)
        {
            startPos = rightHandPoint.position;
        }
        else if (leftHandPoint != null)
        {
            startPos = leftHandPoint.position;
        }
        else
        {
            startPos = transform.position;
        }

        // 如果方向几乎为0，就用面朝方向
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = new Vector2(faceDir, 0f);
        }

        // 初速度：丢掷速度 * 瞄准方向
        Vector2 v0 = dir.normalized * throwSpeed;

        // 2D 重力（会考虑到项目里 Physics2D.gravity 的值）
        Vector2 g = Physics2D.gravity;

        // 确保 LineRenderer 有足够的点
        if (aimLine.positionCount != lineSegments)
        {
            aimLine.positionCount = lineSegments;
        }

        // 采样每个点的轨迹
        for (int i = 0; i < lineSegments; i++)
        {
            float t = i * lineTimeStep;

            // p(t) = p0 + v0 * t + 0.5 * g * t^2
            Vector2 p = (Vector2)startPos + v0 * t + 0.5f * g * (t * t);

            aimLine.SetPosition(i, new Vector3(p.x, p.y, startPos.z));
        }
    }

    // 输入层在 LT/RT 刚按下时调用
    public void BeginAim(bool useLeftHand)
    {
        // 标记哪只手在瞄
        isAiming = true;
        leftAiming = useLeftHand;
        rightAiming = !useLeftHand;

        // 打开瞄准线
        if (aimLine != null)
        {
            aimLine.enabled = true;
        }

        // 初始化一下方向：如果还没右摇杆输入，就按面朝方向
        if (currentAimDir.sqrMagnitude < 0.0001f)
        {
            currentAimDir = new Vector2(faceDir, 0f);
        }

        // 画初始抛物线
        DrawAimArc(currentAimDir);
    }

    // 输入层在右摇杆动的时候调用，把方向传进来
    public void UpdateAimDirection(Vector2 dirFromStick)
    {
        if (dirFromStick.sqrMagnitude > 0.0001f)
        {
            currentAimDir = dirFromStick.normalized;
        }

        if (isAiming)
        {
            DrawAimArc(currentAimDir);
        }
    }

    // 输入层在 LT/RT 松开时调用。这里会真正丢出去，然后关预览
    public void ReleaseAim(InputAction.CallbackContext ctx)
    {
        if (!isAiming) return;

        // 扔东西
        if (leftAiming)
        {
            ThrowFromHand(leftHandPoint, currentAimDir);
        }
        else if (rightAiming)
        {
            ThrowFromHand(rightHandPoint, currentAimDir);
        }

        // 清状态 & 关线
        isAiming = false;
        leftAiming = false;
        rightAiming = false;

        if (aimLine != null)
        {
            aimLine.enabled = false;
        }

    }

    // —— ???? —— 
    Rigidbody2D rb;
    float lastGroundTime;  
    float lastJumpPress;  
    float faceDir = 1f;
    bool dashing; float dashTimer; float dashCdTimer;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void Update()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        if (grounded) lastGroundTime = coyoteTime;
        else lastGroundTime -= Time.deltaTime;

        if (jumpPressed) { lastJumpPress = jumpBuffer; jumpPressed = false; }
        else lastJumpPress -= Time.deltaTime;

        if (dashPressed)
        {
            dashPressed = false;
            if (dashCdTimer <= 0f && !dashing)
            {
                dashing = true;
                dashTimer = dashTime;
                dashCdTimer = dashTime + dashCooldown;
            }
        }
        if (dashCdTimer > 0f) dashCdTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (dashing)
        {
            rb.velocity = new Vector2(faceDir * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) dashing = false;
            return;
        }

        float inputX = moveAxis.x;
        float target = inputX * moveSpeed;
        float diff = target - rb.velocity.x;
        float a = Mathf.Abs(target) > 0.01f ? accel : decel;
        float velX = rb.velocity.x + Mathf.Clamp(diff, -a * Time.fixedDeltaTime, a * Time.fixedDeltaTime);

        float velY = rb.velocity.y;
        if (lastJumpPress > 0f && lastGroundTime > 0f)
        {
            velY = jumpVelocity;
            lastJumpPress = 0f;
            lastGroundTime = 0f;
        }

        rb.velocity = new Vector2(velX, velY);

        // Better Jump?????? / ?????
        if (rb.velocity.y < -0.01f)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityScale - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0.01f)
        {
            bool jumpHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            if (!jumpHeld)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityScale - 1f) * Time.fixedDeltaTime;
            }
        }

        // ??
        if (Mathf.Abs(inputX) > 0.05f)
        {
            faceDir = Mathf.Sign(inputX);
            transform.localScale = new Vector3(faceDir, 1f, 1f);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
#endif
}
