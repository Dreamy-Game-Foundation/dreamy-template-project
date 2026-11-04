using UnityEngine;

namespace Dreamy.UI
{
    [DisallowMultipleComponent]
    public sealed class TweenDelayByIndex : MonoBehaviour
    {
        [SerializeField, Min(0)] private int index;

        private bool hasDelayOverride;
        private float showDelay;
        private float hideDelay;

        public int Index => index;

        private void Awake()
        {
            GetComponentInParent<TweenDelayControl>(true)?.ApplyStaggerDelays();
        }

        /// <summary>Overrides show/hide delays for every tween in this slot's subtree.</summary>
        public void OverrideDelay(bool useDelay, float delayIn, float delayOut)
        {
            hasDelayOverride = useDelay;
            showDelay = Mathf.Max(0f, delayIn);
            hideDelay = Mathf.Max(0f, delayOut);
            foreach (UITweenBase tween in GetComponentsInChildren<UITweenBase>(true))
            {
                if (useDelay) tween.SetDelayOverride(showDelay, hideDelay);
                else tween.ClearDelayOverride();
            }
        }

        public void Apply(int value, float showInterval, float hideInterval, int hideIndex)
        {
            Apply(value, Mathf.Max(0, value) * Mathf.Max(0f, showInterval),
                Mathf.Max(0, hideIndex) * Mathf.Max(0f, hideInterval));
        }

        internal void Apply(int value, float delayIn, float delayOut)
        {
            index = Mathf.Max(0, value);
            OverrideDelay(true, delayIn, delayOut);
        }

        public void Clear() => OverrideDelay(false, 0f, 0f);

        internal TweenTimingData ApplyTo(TweenTimingData timing)
        {
            return hasDelayOverride ? timing.WithDelays(showDelay, hideDelay) : timing;
        }
    }
}
