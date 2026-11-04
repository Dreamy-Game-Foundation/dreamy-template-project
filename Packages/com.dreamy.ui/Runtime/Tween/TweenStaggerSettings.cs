using System;
using UnityEngine;

namespace Dreamy.UI
{
    [Serializable]
    public sealed class TweenStaggerSettings
    {
        [SerializeField] private bool enabled;
        [SerializeField, Min(0f)] private float showInterval = 0.05f;
        [SerializeField, Min(0f)] private float hideInterval = 0.03f;
        [SerializeField] private bool reverseHideOrder = true;

        public bool IsEnabled => enabled;
        public bool ReverseHideOrder => reverseHideOrder;

        public float GetShowDelay(int index)
        {
            return Mathf.Max(0, index) * Mathf.Max(0f, showInterval);
        }

        public float GetHideDelay(int index)
        {
            return Mathf.Max(0, index) * Mathf.Max(0f, hideInterval);
        }
    }
}
