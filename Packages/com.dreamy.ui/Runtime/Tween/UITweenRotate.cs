using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    public sealed class UITweenRotate : UITweenBase
    {
        [SerializeField] private Vector3 inactiveRotation = new(0f, 0f, -12f);

        private Vector3 activeRotation;
        internal override TweenEffectType EffectType => TweenEffectType.Rotate;

        protected override void Setup()
        {
            activeRotation = TargetTransform != null ? TargetTransform.localEulerAngles : Vector3.zero;
        }

        protected override UniTask CreateShowTween()
        {
            Transform target = TargetTransform;
            if (target == null) return UniTask.CompletedTask;
            Tween tween = target.DOLocalRotate(activeRotation, DurationIn).SetEase(EaseIn).SetDelay(DelayIn);
            return Play(tween, Active);
        }

        protected override UniTask CreateHideTween()
        {
            Transform target = TargetTransform;
            if (target == null) return UniTask.CompletedTask;
            Tween tween = target.DOLocalRotate(inactiveRotation, DurationOut).SetEase(EaseOut).SetDelay(DelayOut);
            return Play(tween, Inactive);
        }

        protected override void Active()
        {
            if (TargetTransform != null) TargetTransform.localEulerAngles = activeRotation;
        }

        protected override void Inactive()
        {
            if (TargetTransform != null) TargetTransform.localEulerAngles = inactiveRotation;
        }
    }
}
