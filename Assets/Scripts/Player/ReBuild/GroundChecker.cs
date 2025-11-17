using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("检测设置")]
    [Tooltip("向下射线检测的长度")]
    [SerializeField] private float rayDistance = 0.25f;
    [Tooltip("用于判断地面的Layer名称，默认包含\"Ground\"。")]
    [SerializeField] private string[] groundLayerNames = new[] { "Ground" };

    private LayerMask groundMask;

    private void Awake()
    {
        RefreshGroundMask();
        if (rayDistance < 0f) rayDistance = 0f;
    }

    private void OnValidate()
    {
        RefreshGroundMask();
        if (rayDistance < 0f) rayDistance = 0f;
    }

    /// <summary>
    /// 是否在地面上
    /// </summary>
    public bool IsGrounded()
    {
        // 从角色位置向下发射射线，检测是否命中地面
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundMask);
        return hit.collider != null;
    }

    /// <summary>
    /// 根据配置的Layer名称生成LayerMask
    /// </summary>
    private void RefreshGroundMask()
    {
        if (groundLayerNames == null || groundLayerNames.Length == 0)
        {
            groundMask = LayerMask.GetMask("Ground");
            return;
        }

        int mask = 0;
        foreach (var layerName in groundLayerNames)
        {
            if (string.IsNullOrWhiteSpace(layerName)) continue;
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex >= 0)
            {
                mask |= 1 << layerIndex;
            }
        }

        groundMask = mask != 0 ? mask : LayerMask.GetMask("Ground");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * rayDistance;
        Gizmos.DrawLine(start, end);
        const float lineWidth = 0.02f;
        Gizmos.DrawLine(end, end + Vector3.left * lineWidth);
        Gizmos.DrawLine(end, end + Vector3.right * lineWidth);
    }
#endif
}