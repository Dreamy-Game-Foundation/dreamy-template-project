using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UITweenMoveFade : UITweenBase
    {
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -100f);
        private Vector2 shownPosition;
        private float shownAlpha;
        internal override TweenEffectType EffectType => TweenEffectType.MoveFade;

        protected override void Setup()
        {
            Transform target = TargetTransform;
            if (!(target is RectTransform rect)) throw new MissingComponentException("MoveFade requires a RectTransform.");
            shownPosition = rect.anchoredPosition;
            shownAlpha = TweenFadeMotion.GetGroup(target).alpha;
        }

        protected override UniTask CreateShowTween() => PlayCombined(true);
        protected override UniTask CreateHideTween() => PlayCombined(false);

        private UniTask PlayCombined(bool show)
        {
            Transform target = TargetTransform;
            float duration = show ? DurationIn : DurationOut;
            Tween tween = TweenFadeMotion.Create(target, ((RectTransform)target).DOAnchorPos(show ? shownPosition : shownPosition + hiddenOffset, duration),
                show, shownAlpha, duration, show ? EaseIn : EaseOut, show ? DelayIn : DelayOut);
            return Play(tween, show ? (System.Action)Active : Inactive);
        }

        protected override void Active()
        {
            Transform target = TargetTransform;
            if (target == null) return;
            if (target is RectTransform rect) rect.anchoredPosition = shownPosition;
            TweenFadeMotion.Apply(target, true, shownAlpha);
        }

        protected override void Inactive()
        {
            Transform target = TargetTransform;
            if (target == null) return;
            if (target is RectTransform rect) rect.anchoredPosition = shownPosition + hiddenOffset;
            TweenFadeMotion.Apply(target, false, shownAlpha);
        }
    }
}
