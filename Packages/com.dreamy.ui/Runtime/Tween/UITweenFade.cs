using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UITweenFade : UITweenBase
    {
        [SerializeField] private CanvasGroup canvasGroup;
        internal override TweenEffectType EffectType => TweenEffectType.Fade;

        protected override void Reset()
        {
            base.Reset();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Setup()
        {
            if (!canvasGroup)
            {
                canvasGroup = ResolveTarget(canvasGroup);
            }
        }

        protected override UniTask CreateShowTween()
        {
            CanvasGroup target = ResolveTarget(canvasGroup);
            if (this == null || target == null)
            {
                return UniTask.CompletedTask;
            }

            Tween tween = target.DOFade(1f, DurationIn).SetEase(EaseIn).SetDelay(DelayIn);

            return Play(tween, Active);
        }

        protected override UniTask CreateHideTween()
        {
            CanvasGroup target = ResolveTarget(canvasGroup);
            if (this == null || target == null)
            {
                return UniTask.CompletedTask;
            }

            target.interactable = false;
            Tween tween = target.DOFade(0f, DurationOut).SetEase(EaseOut).SetDelay(DelayOut);

            return Play(tween, Inactive);
        }

        protected override void Active()
        {
            CanvasGroup target = ResolveTarget(canvasGroup);
            if (target != null)
            {
                target.alpha = 1f;
                target.interactable = true;
            }
        }

        protected override void Inactive()
        {
            CanvasGroup target = ResolveTarget(canvasGroup);
            if (target != null)
            {
                target.alpha = 0f;
                target.interactable = false;
            }
        }
    }
}
