using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UITweenScaleFade : UITweenBase
    {
        [SerializeField] private Vector3 hiddenScale = Vector3.zero;
        private Vector3 shownScale;
        private float shownAlpha;
        internal override TweenEffectType EffectType => TweenEffectType.ScaleFade;

        protected override void Setup()
        {
            Transform target = TargetTransform;
            shownScale = target.localScale;
            shownAlpha = TweenFadeMotion.GetGroup(target).alpha;
        }

        protected override UniTask CreateShowTween() => PlayCombined(true);
        protected override UniTask CreateHideTween() => PlayCombined(false);

        private UniTask PlayCombined(bool show)
        {
            Transform target = TargetTransform;
            float duration = show ? DurationIn : DurationOut;
            Tween tween = TweenFadeMotion.Create(target, target.DOScale(show ? shownScale : hiddenScale, duration),
                show, shownAlpha, duration, show ? EaseIn : EaseOut, show ? DelayIn : DelayOut);
            return Play(tween, show ? (System.Action)Active : Inactive);
        }

        protected override void Active()
        {
            Transform target = TargetTransform;
            if (target == null) return;
            target.localScale = shownScale;
            TweenFadeMotion.Apply(target, true, shownAlpha);
        }

        protected override void Inactive()
        {
            Transform target = TargetTransform;
            if (target == null) return;
            target.localScale = hiddenScale;
            TweenFadeMotion.Apply(target, false, shownAlpha);
        }
    }
}
