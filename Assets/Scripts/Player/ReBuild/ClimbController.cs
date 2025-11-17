using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ClimbController : MonoBehaviour
{
    [Header("Climb")]
    public float climbSpeedUp = 4f;
    public float climbSpeedSide = 3f;
    
    private Rigidbody2D rb;
    private bool isClimbing = false;
    private Transform climbAnchor;
    private MonoBehaviour climbSource;

    public bool IsClimbing => isClimbing;
    public MonoBehaviour ClimbSource => climbSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void EnterClimb(Transform anchor, Vector2 upDir, Vector2 rightDir, MonoBehaviour source)
    {
        isClimbing = true;
        climbAnchor = anchor;
        climbSource = source;
        rb.velocity = Vector2.zero;
    }

    public void ExitClimb()
    {
        isClimbing = false;
        climbAnchor = null;
        climbSource = null;
    }

    public void UpdateClimb(Vector2 moveInput, float faceDir)
    {
        if (!isClimbing) return;

        // 更新面向方向
        if (Mathf.Abs(moveInput.x) > 0.05f)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);
        }
    }

    public void FixedUpdateClimb(Vector2 moveInput)
    {
        if (!isClimbing) return;

        float vx = moveInput.x * climbSpeedSide;
        float vy = moveInput.y * climbSpeedUp;
        rb.velocity = new Vector2(vx, vy);
    }
}