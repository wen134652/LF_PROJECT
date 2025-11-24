using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Configuration")]
    [Header("检测设置 (默认值) - 如果未分配配置数据，将使用以下默认值")]
    [Tooltip("向下射线检测的长度")]
    [SerializeField] private float rayDistance = 0.25f;
    [Tooltip("用于判断地面的Layer名称，默认包含\"Ground\"。")]
    [SerializeField] private string[] groundLayerNames = new[] { "Ground" };
    [Tooltip("地面检测射线数量（建议3-5根，更多射线可以提高在斜面上的检测能力）")]
    [Range(1, 10)]
    [SerializeField] private int rayCount = 3;

    // 配置数据引用
    private PlayerConfigData configData;

    [Header("Debug Info (Read Only)")]
    [ReadOnly, SerializeField, Tooltip("当前是否在地面上（只读）")]
    private bool displayIsGrounded = false;
    [ReadOnly, SerializeField, Tooltip("碰撞盒高度（只读）")]
    private float displayColliderHeight = 0f;

    private LayerMask groundMask;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        RefreshGroundMask();
        if (rayDistance < 0f) rayDistance = 0f;
        if (rayCount < 1) rayCount = 1;
        if (rayCount > 10) rayCount = 10;
        
        // 获取 BoxCollider2D 组件
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning($"GroundChecker: {gameObject.name} 上未找到 BoxCollider2D 组件，将使用默认检测方式");
        }
    }

    /// <summary>
    /// 从配置数据初始化参数
    /// </summary>
    public void InitializeFromConfig(PlayerConfigData config)
    {
        if (config == null) return;

        configData = config;

        // 从配置数据读取地面检测参数
        rayDistance = config.rayDistance;
        groundLayerNames = config.groundLayerNames;
        rayCount = config.rayCount;

        // 刷新地面遮罩
        RefreshGroundMask();
        
        // 验证参数
        if (rayDistance < 0f) rayDistance = 0f;
        if (rayCount < 1) rayCount = 1;
        if (rayCount > 10) rayCount = 10;
    }

    private void OnValidate()
    {
        RefreshGroundMask();
        if (rayDistance < 0f) rayDistance = 0f;
        if (rayCount < 1) rayCount = 1;
        if (rayCount > 10) rayCount = 10;
    }

    private void Update()
    {
        // 每帧更新显示值，确保Inspector中显示的是最新状态
        displayIsGrounded = IsGroundedInternal();
    }

    /// <summary>
    /// 是否在地面上
    /// </summary>
    public bool IsGrounded()
    {
        bool isGrounded = IsGroundedInternal();
        // 更新显示值（用于Inspector显示）
        displayIsGrounded = isGrounded;
        return isGrounded;
    }

    /// <summary>
    /// 内部方法：执行实际的地面检测（使用多根射线提高在斜面上的检测能力）
    /// </summary>
    private bool IsGroundedInternal()
    {
        // 获取所有射线的起点位置（均匀分布在碰撞盒底部）
        Vector2[] rayOrigins = GetRaycastOrigins();
        
        // 使用多根射线检测，只要有一根射线命中地面就认为在地面上
        // 这样可以提高在斜面上的检测能力
        foreach (Vector2 rayOrigin in rayOrigins)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);
            if (hit.collider != null)
            {
                return true; // 至少有一根射线命中地面
            }
        }
        
        return false; // 所有射线都没有命中地面
    }
    
    /// <summary>
    /// 获取所有射线检测的起点位置（均匀分布在碰撞盒底部）
    /// </summary>
    private Vector2[] GetRaycastOrigins()
    {
        // 如果 boxCollider 未初始化，尝试获取（用于编辑器模式）
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
        
        Vector2[] origins = new Vector2[rayCount];
        
        // 如果存在 BoxCollider2D，从碰撞盒底部均匀分布发射射线
        if (boxCollider != null)
        {
            // 获取碰撞盒的世界空间边界
            Bounds bounds = boxCollider.bounds;
            
            // 更新显示值：碰撞盒高度
            displayColliderHeight = bounds.size.y;
            
            // 计算碰撞盒底部的宽度
            float bottomWidth = bounds.size.x;
            float bottomY = bounds.min.y;
            float leftX = bounds.min.x;
            
            // 均匀分布射线在碰撞盒底部
            if (rayCount == 1)
            {
                // 只有一根射线，放在中心
                origins[0] = new Vector2(bounds.center.x, bottomY);
            }
            else
            {
                // 多根射线，均匀分布
                // 计算射线之间的间距
                float spacing = bottomWidth / (rayCount - 1);
                
                for (int i = 0; i < rayCount; i++)
                {
                    // 从左边开始，均匀分布
                    float x = leftX + (spacing * i);
                    origins[i] = new Vector2(x, bottomY);
                }
            }
        }
        else
        {
            // 如果没有 BoxCollider2D，回退到使用 transform.position
            displayColliderHeight = 0f;
            for (int i = 0; i < rayCount; i++)
            {
                origins[i] = transform.position;
            }
        }
        
        return origins;
    }
    
    /// <summary>
    /// 获取单根射线检测的起点位置（碰撞盒底部中心，用于向后兼容）
    /// </summary>
    private Vector2 GetRaycastOrigin()
    {
        Vector2[] origins = GetRaycastOrigins();
        // 返回中心位置的射线起点
        return origins[origins.Length / 2];
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
        // 获取所有射线起点
        Vector2[] rayOrigins = GetRaycastOrigins();
        
        // 绘制所有射线
        Gizmos.color = Color.yellow;
        foreach (Vector2 rayOrigin in rayOrigins)
        {
            Vector3 start = rayOrigin;
        Vector3 end = start + Vector3.down * rayDistance;
            
            // 绘制射线
        Gizmos.DrawLine(start, end);
            
            // 绘制射线起点的标记（小圆点）
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(start, 0.02f);
            
            // 绘制射线终点的标记
            Gizmos.color = Color.yellow;
        const float lineWidth = 0.02f;
        Gizmos.DrawLine(end, end + Vector3.left * lineWidth);
        Gizmos.DrawLine(end, end + Vector3.right * lineWidth);
        }
        
        // 如果有碰撞盒，绘制碰撞盒底部位置标记
        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Bounds bounds = boxCollider.bounds;
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            Gizmos.DrawWireSphere(bottomCenter, 0.05f);
            
            // 绘制碰撞盒底部边界线，方便查看射线分布
            Gizmos.color = Color.green;
            Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
            Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z);
            Gizmos.DrawLine(bottomLeft, bottomRight);
        }
    }
#endif
}