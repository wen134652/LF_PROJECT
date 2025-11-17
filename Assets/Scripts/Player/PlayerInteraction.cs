using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


    public class PlayerInteraction : MonoBehaviour
    {
        [Header("输入键")]
        public KeyCode interactKey = KeyCode.E;

        [Header("选择策略")]
        public float maxInteractDistance = 3f;

        [Header("可选：把 UI 文本的 SetText/SetString 之类方法拖进来")]
        public UnityEvent<string> OnPromptChanged;

        private readonly Dictionary<IInteractable, Vector3> _candidates = new();
        private IInteractable _current;
        private Transform _player;

        void Awake() => _player = transform;

        public void AddCandidate(IInteractable it, Vector2 hintPos)
        {
            if (!_candidates.ContainsKey(it)) _candidates.Add(it, hintPos);
        }

        public void RemoveCandidate(IInteractable it)
        {
            if (_candidates.ContainsKey(it)) _candidates.Remove(it);
            if (_current == it) _current = null;
        }

        void Update()
        {
            PickBest();

            if (_current != null && _current.CanInteract(_player))
                OnPromptChanged?.Invoke(_current.Prompt);
            else
                OnPromptChanged?.Invoke(string.Empty);

            if (_current != null && Input.GetKeyDown(interactKey))
            {
                if (_current.CanInteract(_player)) _current.Interact(_player);
            }
        }

        void PickBest()
        {
            float best = float.MaxValue;
            IInteractable bestIt = null;

            foreach (var kv in _candidates)
            {
                float d = Vector2.Distance(_player.position, kv.Value);
                if (d > maxInteractDistance) continue;
                if (d < best) { best = d; bestIt = kv.Key; }
            }
            _current = bestIt;
        }
    }

