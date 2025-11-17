using UnityEngine;


    [RequireComponent(typeof(Collider2D))]
    public class InteractableTrigger : MonoBehaviour
    {
        private IInteractable interactable;
        [Tooltip("仅响应这个Tag的对象；留空=不限制")]
        public string playerTag = "Player";

        void Awake()
        {
            // 在本物体/父/子上找 IInteractable
            interactable = GetComponent<IInteractable>() ??
                           GetComponentInParent<IInteractable>() ??
                           GetComponentInChildren<IInteractable>();

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

    void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("进入");
            if (interactable == null) return;
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

            var player = other.transform;
            interactable.SetPlayer(player);

            var mgr = player.GetComponentInParent<PlayerInteraction>();
            if (mgr) mgr.AddCandidate(interactable, transform.position);
        }

    void OnTriggerExit2D(Collider2D other)
        {
            if (interactable == null) return;
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

            var player = other.transform;
            interactable.ClearPlayer(player);

            var mgr = player.GetComponentInParent<PlayerInteraction>();
            if (mgr) mgr.RemoveCandidate(interactable);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            if (TryGetComponent<Collider2D>(out var c))
            {
                //if (c is BoxCollider2D b) Gizmos.dr(b.center, b.size);
                //else Gizmos.DrawWireCube(Vector2.zero, Vector2.one * 0.3f);
            }
        }
#endif
    }

