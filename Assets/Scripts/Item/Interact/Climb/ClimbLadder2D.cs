using UnityEngine;

public class ClimbLadder2D : MonoBehaviour, IInteractable
{
    [Header("Climb Enter")]
    public Transform snapAnchor;            // 吸附参考点（放在梯子中线胸口高）
    [SerializeField] private string prompt = "按 E 拾取";
    public int priority = 5;
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
        var ctrl = player.GetComponent<ClimbController>();
        if (ctrl == null) return;
        if (!ctrl.IsClimbing)
        {
            var anchor = snapAnchor ? snapAnchor : null;
            ctrl.EnterClimb(anchor, Vector2.up, Vector2.right, this);
        }
        else
        {
            if (ctrl.ClimbSource == (MonoBehaviour)this)
                ctrl.ExitClimb();
        }
    }
    private void OnTriggerExit2D(Collider2D player)
    {
        var ctrl = player.GetComponent<ClimbController>();
        if (ctrl && ctrl.IsClimbing && ctrl.ClimbSource == (MonoBehaviour)this)
        {
            ctrl.ExitClimb();
        }
    }
}
