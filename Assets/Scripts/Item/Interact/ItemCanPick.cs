using UnityEngine;

public class ItemCanPick : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "按 E 拾取";
    InteractableTrigger trigger;
    private void OnEnable()
    {
        trigger = GetComponentInChildren<InteractableTrigger>();
        if (trigger)
        {
            trigger.enabled = true;
        }
        
    }
    private void OnDisable()
    {
        trigger = GetComponentInChildren<InteractableTrigger>();
        if (trigger)
        {
            trigger.enabled = false;
        }
    }
    private Transform _player;
    public string Prompt => prompt;
    public void SetPlayer(Transform player) => _player = player;
    public void ClearPlayer(Transform player) { if (_player == player) _player = null; }
    //之后把这个的引用都删了
    public bool CanInteract(Transform player)
    {
        return true;
    }
    public void Interact(Transform player)
    {
        //仅作测试用不代表真实拾取逻辑
        Transform leftHand = player.Find("LeftHand");
        transform.position = leftHand.position;
        transform.SetParent(leftHand);
        Rigidbody2D rd = GetComponent<Rigidbody2D>();
        rd.bodyType = RigidbodyType2D.Kinematic;
        enabled = false;
        Debug.Log($"[Pickup] {name} 被 {player.name} 拾取");
        //Destroy(gameObject);
    }
}

