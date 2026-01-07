using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("选择策略")]
    public float maxInteractDistance = 3f;

    [Header("可选：把 UI 文本的 SetText/SetString 之类方法拖进来")]
    public UnityEvent<string> OnPromptChanged;

    [Header("长按阈值（仅拾取用）")]
    public float holdThreshold = 0.5f;

    // 所有候选（由 InteractableTrigger 维护进出）
    private readonly Dictionary<IInteractable, Vector3> _candidates = new();

    // 分开计算：拾取目标 vs 交互目标
    private IInteractable _currentPickup;
    private IInteractable _currentInteract;

    // 拾取长按锁定
    private IInteractable _active;      // 被拾取键按下时锁定的目标
    private Transform _player;

    private bool _pressing = false;     // 是否正在按住拾取键（仅拾取用）
    private float _pressStartTime = -1f
;

    void Awake() => _player = transform;

    public void AddCandidate(IInteractable it, Vector2 hintPos)
    {
        if(!_candidates.ContainsKey(it)) _candidates.Add(it, hintPos);
    }

    public void RemoveCandidate(IInteractable it)
    {
        if (_candidates.ContainsKey(it)) _candidates.Remove(it);

        if (_currentPickup == it) _currentPickup = null
;
        if (_currentInteract == it) _currentInteract = null
;
        if (_active == it) _active = null
;
    }

    void Update()
    {
        // 拾取长按期间不重新选目标（避免锁定目标飘）
        if (!_pressing)
            PickBestBoth();

        // UI 提示策略：优先显示拾取，否则显示交互
        var show = _currentPickup != null ? _currentPickup : _currentInteract;

        if (show != null && show.CanInteract(_player))
            OnPromptChanged?.Invoke(show.Prompt);
        else
            OnPromptChanged?.Invoke(string.Empty);
    }

    // 交互键（非拾取）：按一下立刻触发
    public void Interact(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        if (_currentInteract == null) return;

        if (_currentInteract.CanInteract(_player))
            _currentInteract.Interact(_player);
    }

    // 拾取键：短/长按（仅拾取类）
    public void Pickup(InputAction.CallbackContext ctx)
    {
        // 按下：锁定拾取目标并开始计时
        if (ctx.started)
        {
            if (_currentPickup == null) return;

            _pressing =true;
            _pressStartTime = Time.time;
            _active = _currentPickup;
            return;
        }

        // 松开：结算短/长按
        if (_pressing && ctx.canceled)
        {
            _pressing =false;

            float held = Mathf.Max(0f, Time.time - _pressStartTime);
            _pressStartTime =-1f;

            if (_active != null&& _active.CanInteract(_player))
            {
                // 只有拾取暂时有短/长按：优先走 IShortLongInteractable
                if (_active is IShortLongInteractable sl)
                {
                    if (held >= holdThreshold) sl.LongInteract(_player);
                    else
                        sl.ShortInteract(_player);
                }
                else
                {
                    // 如果某些拾取物暂时没实现短长按接口，就退回普通拾取
                    _active.Interact(_player);
                }
            }

            _active =null
;
        }
    }

    // 同时选出：拾取最佳 + 交互最佳
    void PickBestBoth
()
    {
        float bestPickup = float.MaxValue;
        float bestInteract = float.MaxValue;

        IInteractable bestPickupIt =null;
        IInteractable bestInteractIt =null;

        Vector2 p = (Vector2)_player.position;

        foreach (var kv in _candidates)
        {
            var it = kv.Key;
            float d = Vector2.Distance(p, (Vector2)kv.Value);
            if (d > maxInteractDistance) continue;

            bool isPickup = it is IPickupInteractable;

            if (isPickup)
            {
                if (d < bestPickup) { bestPickup = d; bestPickupIt = it; }
            }
            else
            {
                if(d < bestInteract) { bestInteract = d; bestInteractIt = it; }
            }
        }

        _currentPickup = bestPickupIt;
        _currentInteract = bestInteractIt;
    }
}