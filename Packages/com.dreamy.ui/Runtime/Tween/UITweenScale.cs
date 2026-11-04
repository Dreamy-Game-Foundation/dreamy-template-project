using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    public class UITweenScale : UITweenBase
    {
        [SerializeField] private float inactiveScale;
        [SerializeField] private float activeScale = 1f;
        internal override TweenEffectType EffectType => TweenEffectType.Scale;

        protected override UniTask CreateShowTween()
        {
            Transform target = TargetTransform;
            if (target == null) return UniTask.CompletedTask;
            Tween tween = target.DOScale(Vector3.one * activeScale, DurationIn).SetEase(EaseIn).SetDelay(DelayIn);

            return Play(tween, Active);
        }

        protected override UniTask CreateHideTween()
        {
            Transform target = TargetTransform;
            if (target == null) return UniTask.CompletedTask;
            Tween tween = target.DOScale(Vector3.one * inactiveScale, DurationOut).SetEase(EaseOut).SetDelay(DelayOut);

            return Play(tween, Inactive);
        }

        protected override void Active()
        {
            if (TargetTransform != null) TargetTransform.localScale = Vector3.one * activeScale;
        }

        protected override void Inactive()
        {
            if (TargetTransform != null) TargetTransform.localScale = Vector3.one * inactiveScale;
        }
    }
}
