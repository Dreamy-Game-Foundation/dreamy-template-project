using UnityEngine;

namespace Dreamy.UI
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class TweenDelayControl : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float showInterval = 0.05f;
        [SerializeField, Min(0f)] private float hideInterval = 0.03f;
        [SerializeField, Min(0f)] private float startDelay;
        [SerializeField] private bool reverseHideOrder = true;

        private void Awake()
        {
            ApplyStaggerDelays();
        }

        private void OnValidate()
        {
            showInterval = Mathf.Max(0f, showInterval);
            hideInterval = Mathf.Max(0f, hideInterval);
            startDelay = Mathf.Max(0f, startDelay);
        }

        /// <summary>Recalculates delays, including inactive and runtime-spawned slots.</summary>
        [ContextMenu("Apply Tween Delays")]
        public void ApplyStaggerDelays()
        {
            TweenDelayByIndex[] entries = GetComponentsInChildren<TweenDelayByIndex>(true);
            int count = entries.Length;
            for (int index = 0; index < count; index++)
            {
                int hideIndex = reverseHideOrder ? count - index - 1 : index;
                entries[index].Apply(index,
                    Mathf.Max(0f, startDelay) + index * Mathf.Max(0f, showInterval),
                    hideIndex * Mathf.Max(0f, hideInterval));
            }
        }

        public void ApplyDelays() => ApplyStaggerDelays();

        [ContextMenu("Clear Tween Delays")]
        public void ClearDelays()
        {
            foreach (TweenDelayByIndex entry in GetComponentsInChildren<TweenDelayByIndex>(true))
                entry.Clear();
        }
    }
}
