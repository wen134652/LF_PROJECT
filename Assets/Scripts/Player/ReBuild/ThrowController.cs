using UnityEngine;

public class ThrowController : MonoBehaviour
{
    [Header("Hand")]
    public Transform leftHandPoint;
    public Transform rightHandPoint;
    public float throwSpeed = 12f;

    [Header("Aim Line")]
    public LineRenderer aimLine;
    public int lineSegments = 20;
    public float lineTimeStep = 0.05f;

    private Vector2 currentAimDir = Vector2.right;
    private bool isAiming = false;
    private bool leftAiming = false;
    private Throwable throwableItem;
    private float faceDir = 1f;

    public bool IsAiming => isAiming;
    public float FaceDir { set => faceDir = value; }

    private void Awake()
    {
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    public void BeginAim(bool useLeftHand)
    {
        // 检查是否可以投掷
        if (!TryGetThrowableFromHand(useLeftHand, out throwableItem))
        {
            return;
        }

        isAiming = true;
        leftAiming = useLeftHand;

        // 初始化方向
        if (currentAimDir.sqrMagnitude < 0.0001f)
        {
            currentAimDir = new Vector2(faceDir, 0f);
        }

        // 显示瞄准线
        if (aimLine != null)
        {
            aimLine.enabled = true;
        }

        DrawAimArc(currentAimDir);
    }

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

    public void ReleaseAim()
    {
        if (!isAiming) return;

        // 投掷物品
        Transform handPoint = leftAiming ? leftHandPoint : rightHandPoint;
        if (handPoint != null)
        {
            ThrowFromHand(handPoint, currentAimDir);
        }

        // 清理状态
        isAiming = false;
        leftAiming = false;
        throwableItem = null;

        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    private bool TryGetThrowableFromHand(bool useLeftHand, out Throwable throwable)
    {
        Transform handPoint = useLeftHand ? leftHandPoint : rightHandPoint;
        throwable = null;

        if (handPoint == null)
        {
            Debug.LogWarning($"{ (useLeftHand ? "左手" : "右手") } 手点未设置");
            return false;
        }

        if (handPoint.childCount != 1)
        {
            Debug.Log($"{ (useLeftHand ? "左手" : "右手") } 没东西丢");
            return false;
        }

        throwable = handPoint.GetChild(0).GetComponent<Throwable>();
        if (throwable == null)
        {
            Debug.Log("东西不能丢");
            return false;
        }

        return true;
    }

    private void ThrowFromHand(Transform handPoint, Vector2 aimDir)
    {
        if (handPoint == null) return;

        // 如果方向几乎为0，使用面向方向
        if (aimDir.sqrMagnitude < 0.0001f)
        {
            aimDir = new Vector2(faceDir, 0f);
        }

        // 如果有 Throwable 组件，使用其投掷方法
        if (leftAiming && throwableItem != null)
        {
            throwableItem.transform.SetParent(null);
            throwableItem.ThrowOut(aimDir);
            return;
        }

        // 否则使用预制体实例化（如果需要）
        // 这部分逻辑可以根据项目需求调整
    }

    private void DrawAimArc(Vector2 dir)
    {
        if (aimLine == null) return;

        // 确定起点
        Vector3 startPos = GetAimStartPosition();

        // 如果方向几乎为0，使用面向方向
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = new Vector2(faceDir, 0f);
        }

        // 初速度
        Vector2 v0 = dir.normalized * throwSpeed;

        // 重力
        Vector2 g = Physics2D.gravity;

        // 设置 LineRenderer 点数
        if (aimLine.positionCount != lineSegments)
        {
            aimLine.positionCount = lineSegments;
        }

        // 计算轨迹点
        for (int i = 0; i < lineSegments; i++)
        {
            float t = i * lineTimeStep;
            Vector2 p = (Vector2)startPos + v0 * t + 0.5f * g * (t * t);
            aimLine.SetPosition(i, new Vector3(p.x, p.y, startPos.z));
        }
    }

    private Vector3 GetAimStartPosition()
    {
        // 优先使用当前瞄准的手
        if (leftAiming && leftHandPoint != null)
        {
            return leftHandPoint.position;
        }
        if (!leftAiming && rightHandPoint != null)
        {
            return rightHandPoint.position;
        }

        // 备用方案
        if (rightHandPoint != null)
        {
            return rightHandPoint.position;
        }
        if (leftHandPoint != null)
        {
            return leftHandPoint.position;
        }

        return transform.position;
    }
}