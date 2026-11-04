using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    public class UITweenMove : UITweenBase
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Vector2 offset;

        private Vector2 activePosition;
        private Vector2 inactivePosition;
        internal override TweenEffectType EffectType => TweenEffectType.Move;

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
                    $"{nameof(UITweenMove)} requires a RectTransform.");
            }

            activePosition = target.anchoredPosition;
            inactivePosition = activePosition + offset;
        }

        protected override UniTask CreateShowTween()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target == null)
            {
                return UniTask.CompletedTask;
            }
            Tween tween = target.DOAnchorPos(activePosition, DurationIn).SetEase(EaseIn).SetDelay(DelayIn);

            return Play(tween, Active);
        }

        protected override UniTask CreateHideTween()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target == null)
            {
                return UniTask.CompletedTask;
            }
            Tween tween = target.DOAnchorPos(inactivePosition, DurationOut).SetEase(EaseOut).SetDelay(DelayOut);

            return Play(tween, Inactive);
        }

        protected override void Active()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target != null)
            {
                target.anchoredPosition = activePosition;
            }
        }

        protected override void Inactive()
        {
            RectTransform target = ResolveTarget(rectTransform);
            if (target != null)
            {
                target.anchoredPosition = inactivePosition;
            }
        }
    }
}
