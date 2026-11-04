using DG.Tweening;
using UnityEngine;


namespace Dreamy.UI
{
    [CreateAssetMenu(
        fileName = "TweenSettings",
        menuName = "Dreamy/UI/Tween Preset")]
    public sealed class TweenSettings : ScriptableObject
    {
        public Ease EaseIn = Ease.OutBack;
        public Ease EaseOut = Ease.InBack;
        public float DurationIn = 0.25f;
        public float DurationOut = 0.2f;
        public float DelayIn;
        public float DelayOut;

        private void OnValidate()
        {
            DurationIn = Mathf.Max(0f, DurationIn);
            DurationOut = Mathf.Max(0f, DurationOut);
            DelayIn = Mathf.Max(0f, DelayIn);
            DelayOut = Mathf.Max(0f, DelayOut);
        }
    }
}
