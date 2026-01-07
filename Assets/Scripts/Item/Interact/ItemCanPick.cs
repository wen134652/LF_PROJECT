using UnityEngine;

public class ItemCanPick : MonoBehaviour, IPickupInteractable, IShortLongInteractable
{
    [SerializeField] private string prompt = "按 E 拿起（长按入包）";
    InteractableTrigger trigger;

    public ItemSO item;

    private Transform _player; // 由触发器注入
    public string Prompt => prompt;

    private void OnEnable()
    {
        trigger = GetComponentInChildren<InteractableTrigger>();
        if (trigger) trigger.enabled = true;
    }

    private void OnDisable()
    {
        trigger = GetComponentInChildren<InteractableTrigger>();
        if (trigger) trigger.enabled = false;
    }

    // 触发器进入/离开时由你的系统调用
    public void SetPlayer(Transform player) => _player = player;
    public void ClearPlayer(Transform player) { if (_player == player) _player = null; }

    // 简化：是否可交互（需要更严格就加距离/角度判断）
    public bool CanInteract(Transform player) => true;

    // 兼容旧逻辑：如果外部仍直接调 Interact()，默认当“短按拿起”
    public void Interact(Transform player)
    {
        ShortInteract(player);
    }

    // ================= 短/长按分流 =================

    // 短按：拿在手上（不入包，不销毁）
    public void ShortInteract(Transform player)
    {
        var p = player != null ? player : _player;
        if (p == null) return;

        var leftHand = p.Find("LeftHand");
        if (leftHand == null)
        {
            leftHand = p;
        }

        // 2) 绑定到手上并“失去物理/碰撞”
        transform.SetParent(leftHand, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 3) 可选：禁用本脚本避免再次被交互（手持状态）
        enabled = false;

        Debug.Log($"[Pickup-Short] {name} 被 {p.name} 拿在手上");
    }

    // 长按：直接放入背包并从场景移除
    public void LongInteract(Transform player)
    {
        var p = player != null ? player : _player;
        if (p == null) return;

        // 你现有的入包逻辑（保持不变）
        var invManager = GameObject.FindGameObjectWithTag("InventoryManager");
        if (invManager != null)
        {
            var inventory = invManager.GetComponent<InventoryGrid>();
            if (inventory != null && item != null)
            {
                inventory.PlaceNewItemWithNoPosition(item);
                Debug.Log($"[Pickup-Long] {name} 放入背包");
                Destroy(gameObject);
                return;
            }
        }

        // 兜底：如果没拿到 Inventory，就先隐藏
        Debug.LogWarning("[Pickup-Long] 未找到 InventoryManager 或 item 为空，先隐藏物体");
        gameObject.SetActive(false);
    }
}
