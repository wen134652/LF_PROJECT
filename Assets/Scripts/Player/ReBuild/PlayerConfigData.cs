using UnityEngine;

/// <summary>
/// 玩家配置数据资产，存储所有角色相关的配置参数
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerConfig", menuName = "Player/Player Config Data", order = 1)]
public class PlayerConfigData : ScriptableObject
{
    [Header("========== 移动功能配置 ==========")]
    [Tooltip("基础移动速度")]
    public float moveSpeed = 8f;
    [Tooltip("移动加速度")]
    public float accel = 60f;
    [Tooltip("移动减速度")]
    public float decel = 80f;

    [Header("========== 蹲下移动功能配置 ==========")]
    [Tooltip("蹲下时的移动速度")]
    public float crouchMoveSpeed = 4f;
    [Tooltip("蹲下时的移动加速度")]
    public float crouchAccel = 30f;
    [Tooltip("蹲下时的移动减速度")]
    public float crouchDecel = 40f;

    [Header("========== 冲刺移动功能配置 ==========")]
    [Tooltip("冲刺速度")]
    public float dashSpeed = 16f;
    [Tooltip("冲刺加速度")]
    public float dashAccel = 200f;
    [Tooltip("冲刺持续时间（秒）")]
    public float dashTime = 0.15f;
    [Tooltip("冲刺冷却时间（秒）")]
    public float dashCooldown = 0.40f;

    [Header("========== 跳跃功能配置 ==========")]
    [Tooltip("跳跃时的初始向上速度")]
    public float jumpVelocity = 12f;
    [Tooltip("离开地面后仍可跳跃的时间窗口（秒）- Coyote Time")]
    public float coyoteTime = 0.10f;
    [Tooltip("提前按下跳跃键的缓冲时间（秒）- Jump Buffer")]
    public float jumpBuffer = 0.10f;
    [Tooltip("按住跳跃键时维持向上速度的最大时间（秒）")]
    public float jumpHoldTime = 0.2f;

    [Header("========== 重力和下落功能配置 ==========")]
    [Tooltip("下落时的重力加速度倍数")]
    public float fallGravityScale = 2.2f;
    [Tooltip("松开跳跃键时的额外向下加速度倍数")]
    public float earlyReleaseGravityScale = 2.8f;
    [Tooltip("最大下落速度（负值，单位/秒）")]
    public float maxFallSpeed = -30f;

    [Header("========== 地面检测功能配置 ==========")]
    [Tooltip("向下射线检测的长度")]
    public float rayDistance = 0.25f;
    [Tooltip("用于判断地面的Layer名称数组")]
    public string[] groundLayerNames = new[] { "Ground" };
    [Tooltip("地面检测射线数量（建议3-5根，更多射线可以提高在斜面上的检测能力）")]
    [Range(1, 10)]
    public int rayCount = 3;
}

