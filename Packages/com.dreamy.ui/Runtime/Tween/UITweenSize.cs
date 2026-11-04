using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    public sealed class UITweenSize : UITweenBase
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Vector2 inactiveSize;

        private Vector2 activeSize;
        internal override TweenEffectType EffectType => TweenEffectType.Size;

        protected override void Reset()
        {
            base.Reset();
            rectTransform = transform as RectTransform;
        }

        protected override void Setup()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target == null)
            {
                throw new MissingComponentException(
                    $"{nameof(UITweenSize)} requires a RectTransform.");
            }

            activeSize = target.sizeDelta;
        }

        protected override UniTask CreateShowTween()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target == null)
            {
                return UniTask.CompletedTask;
            }
            Tween tween = target.DOSizeDelta(activeSize, DurationIn).SetEase(EaseIn).SetDelay(DelayIn);
            return Play(tween, Active);
        }

        protected override UniTask CreateHideTween()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target == null)
            {
                return UniTask.CompletedTask;
            }
            Tween tween = target.DOSizeDelta(inactiveSize, DurationOut).SetEase(EaseOut).SetDelay(DelayOut);
            return Play(tween, Inactive);
        }

        protected override void Active()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target != null)
            {
                target.sizeDelta = activeSize;
            }
        }

        protected override void Inactive()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target != null)
            {
                target.sizeDelta = inactiveSize;
            }
        }
    }
}
