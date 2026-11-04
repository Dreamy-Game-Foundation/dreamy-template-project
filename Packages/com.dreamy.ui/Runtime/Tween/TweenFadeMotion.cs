using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    internal static class TweenFadeMotion
    {
        internal static CanvasGroup GetGroup(Component target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            return group != null ? group : target.gameObject.AddComponent<CanvasGroup>();
        }

        internal static Tween Create(Transform target, Tween motion, bool show,
            float shownAlpha, float duration, Ease ease, float delay)
        {
            CanvasGroup group = GetGroup(target);
            // Hidden and transitioning UI must not intercept input.
            group.interactable = false;
            group.blocksRaycasts = false;
            return DOTween.Sequence()
                .Join(motion.SetEase(ease))
                .Join(group.DOFade(show ? shownAlpha : 0f, duration).SetEase(Ease.Linear))
                .SetDelay(delay)
                .SetTarget(target);
        }

        internal static void Apply(Transform target, bool show, float shownAlpha)
        {
            CanvasGroup group = GetGroup(target);
            group.alpha = show ? shownAlpha : 0f;
            group.interactable = show;
            group.blocksRaycasts = show;
        }
    }
}
