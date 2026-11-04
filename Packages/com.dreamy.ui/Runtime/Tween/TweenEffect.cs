using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamy.UI
{
    public enum TweenEffectType
    {
        Scale,
        Fade,
        Move,
        Rotate,
        Size,
        MoveFade,
        ScaleFade
    }

    [Serializable]
    public sealed class TweenOverrideSettings
    {
        [SerializeField] private bool overrideEaseIn;
        [SerializeField] private Ease easeIn = Ease.OutBack;
        [SerializeField] private bool overrideEaseOut;
        [SerializeField] private Ease easeOut = Ease.InBack;
        [SerializeField] private bool overrideDurationIn;
        [SerializeField, Min(0f)] private float durationIn = 0.25f;
        [SerializeField] private bool overrideDurationOut;
        [SerializeField, Min(0f)] private float durationOut = 0.2f;
        [SerializeField] private bool overrideDelayIn;
        [SerializeField, Min(0f)] private float delayIn;
        [SerializeField] private bool overrideDelayOut;
        [SerializeField, Min(0f)] private float delayOut;

        internal TweenTimingData Resolve(TweenSettings preset)
        {
            return TweenSettingsResolver.Resolve(
                preset,
                overrideEaseIn, easeIn,
                overrideEaseOut, easeOut,
                overrideDurationIn, durationIn,
                overrideDurationOut, durationOut,
                overrideDelayIn ? delayIn : preset ? preset.DelayIn : 0f,
                overrideDelayOut ? delayOut : preset ? preset.DelayOut : 0f);
        }
    }

    [Serializable]
    public abstract class UITweenDefinition : ITween
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private TweenSettings settings;
        [FormerlySerializedAs("timing")]
        [SerializeField] private TweenOverrideSettings overrideSettings = new TweenOverrideSettings();

        [NonSerialized] private TweenPlayback playback;
        [NonSerialized] private Transform initializedTarget;
        [NonSerialized] private Transform target;
        [NonSerialized] private TweenSettings inheritedPreset;
        [NonSerialized] private Component owner;

        public bool IsAutoRun => false;
        public bool IsEnabled => enabled;
        public abstract TweenEffectType Type { get; }

        public bool ApplyPresetIfMissing(TweenSettings value)
        {
            if (settings != null || value == null)
            {
                return false;
            }

            settings = value;
            return true;
        }

        internal void Bind(Transform value, TweenSettings inheritedSettings, Component tweenOwner)
        {
            if (target != value)
            {
                initializedTarget = null;
            }

            target = value;
            inheritedPreset = inheritedSettings;
            owner = tweenOwner;
        }

        public UniTask Init()
        {
            if (target == null || initializedTarget == target)
            {
                return UniTask.CompletedTask;
            }

            initializedTarget = target;
            CaptureShownState(target);
            ApplyHidden(target);
            return UniTask.CompletedTask;
        }

        public UniTask Show(CancellationToken token)
        {
            return Play(true, token);
        }

        public UniTask Hide(CancellationToken token)
        {
            return Play(false, token);
        }

        public void Kill()
        {
            playback?.Kill();
        }

        protected abstract void CaptureShownState(Transform target);
        protected abstract Tween CreateTween(Transform target, bool show, TweenTimingData timing);
        protected abstract void ApplyShown(Transform target);
        protected abstract void ApplyHidden(Transform target);

        private TweenTimingData ResolveTiming()
        {
            TweenTimingData timing = overrideSettings.Resolve(settings ? settings : inheritedPreset);
            TweenDelayByIndex delayByIndex = target != null ? target.GetComponentInParent<TweenDelayByIndex>(true) : null;
            if (delayByIndex != null)
            {
                timing = delayByIndex.ApplyTo(timing);
            }

            return timing;
        }

        private UniTask Play(bool show, CancellationToken token)
        {
            if (!enabled || target == null || owner == null)
            {
                return UniTask.CompletedTask;
            }

            Init();
            TweenTimingData resolved = ResolveTiming();
            try
            {
                Tween tween = CreateTween(target, show, resolved);
                return Playback.Play(
                    tween,
                    () =>
                    {
                        if (target == null)
                        {
                            return;
                        }

                        if (show) ApplyShown(target);
                        else ApplyHidden(target);
                    },
                    owner).AttachExternalCancellation(token);
            }
            catch (MissingReferenceException)
            {
                return UniTask.CompletedTask;
            }
        }

        private TweenPlayback Playback => playback ??= new TweenPlayback();
    }

    [Serializable]
    public sealed class ScaleTweenEffect : UITweenDefinition
    {
        [SerializeField] private Vector3 shownScale = Vector3.one;
        [SerializeField] private Vector3 hiddenScale = Vector3.zero;
        public override TweenEffectType Type => TweenEffectType.Scale;
        protected override void CaptureShownState(Transform target) => shownScale = target.localScale;
        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing) =>
            target.DOScale(show ? shownScale : hiddenScale, show ? timing.DurationIn : timing.DurationOut)
                .SetEase(show ? timing.EaseIn : timing.EaseOut)
                .SetDelay(show ? timing.DelayIn : timing.DelayOut);
        protected override void ApplyShown(Transform target) => target.localScale = shownScale;
        protected override void ApplyHidden(Transform target) => target.localScale = hiddenScale;
    }

    [Serializable]
    public sealed class FadeTweenEffect : UITweenDefinition
    {
        [SerializeField, Range(0f, 1f)] private float shownAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float hiddenAlpha;
        [SerializeField] private bool controlInteractable = true;
        public override TweenEffectType Type => TweenEffectType.Fade;
        protected override void CaptureShownState(Transform target)
        {
            CanvasGroup group = GetOrAddCanvasGroup(target);
            shownAlpha = group.alpha;
        }
        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing)
        {
            CanvasGroup group = GetOrAddCanvasGroup(target);
            if (!show && controlInteractable) group.interactable = false;
            return group.DOFade(show ? shownAlpha : hiddenAlpha, show ? timing.DurationIn : timing.DurationOut)
                .SetEase(show ? timing.EaseIn : timing.EaseOut)
                .SetDelay(show ? timing.DelayIn : timing.DelayOut);
        }
        protected override void ApplyShown(Transform target)
        {
            CanvasGroup group = GetOrAddCanvasGroup(target);
            group.alpha = shownAlpha;
            if (controlInteractable)
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }
        protected override void ApplyHidden(Transform target)
        {
            CanvasGroup group = GetOrAddCanvasGroup(target);
            group.alpha = hiddenAlpha;
            if (controlInteractable)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(Component target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            return group != null ? group : target.gameObject.AddComponent<CanvasGroup>();
        }
    }

    [Serializable]
    public sealed class MoveTweenEffect : UITweenDefinition
    {
        [SerializeField] private Vector2 hiddenOffset;
        [NonSerialized] private Vector2 shownPosition;
        public override TweenEffectType Type => TweenEffectType.Move;
        protected override void CaptureShownState(Transform target)
        {
            RectTransform rect = target as RectTransform;
            if (rect != null) shownPosition = rect.anchoredPosition;
        }
        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing)
        {
            RectTransform rect = target as RectTransform;
            return rect == null ? null : rect.DOAnchorPos(show ? shownPosition : shownPosition + hiddenOffset,
                    show ? timing.DurationIn : timing.DurationOut)
                .SetEase(show ? timing.EaseIn : timing.EaseOut).SetDelay(show ? timing.DelayIn : timing.DelayOut);
        }
        protected override void ApplyShown(Transform target) { if (target is RectTransform rect) rect.anchoredPosition = shownPosition; }
        protected override void ApplyHidden(Transform target) { if (target is RectTransform rect) rect.anchoredPosition = shownPosition + hiddenOffset; }
    }

    [Serializable]
    public sealed class RotateTweenEffect : UITweenDefinition
    {
        [SerializeField] private Vector3 hiddenRotation = new Vector3(0f, 0f, -12f);
        [NonSerialized] private Vector3 shownRotation;
        public override TweenEffectType Type => TweenEffectType.Rotate;
        protected override void CaptureShownState(Transform target) => shownRotation = target.localEulerAngles;
        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing) =>
            target.DOLocalRotate(show ? shownRotation : hiddenRotation, show ? timing.DurationIn : timing.DurationOut)
                .SetEase(show ? timing.EaseIn : timing.EaseOut).SetDelay(show ? timing.DelayIn : timing.DelayOut);
        protected override void ApplyShown(Transform target) => target.localEulerAngles = shownRotation;
        protected override void ApplyHidden(Transform target) => target.localEulerAngles = hiddenRotation;
    }

    [Serializable]
    public sealed class SizeTweenEffect : UITweenDefinition
    {
        [SerializeField] private Vector2 hiddenSize;
        [NonSerialized] private Vector2 shownSize;
        public override TweenEffectType Type => TweenEffectType.Size;
        protected override void CaptureShownState(Transform target)
        {
            if (target is RectTransform rect) shownSize = rect.sizeDelta;
        }
        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing)
        {
            RectTransform rect = target as RectTransform;
            return rect == null ? null : rect.DOSizeDelta(show ? shownSize : hiddenSize,
                    show ? timing.DurationIn : timing.DurationOut)
                .SetEase(show ? timing.EaseIn : timing.EaseOut).SetDelay(show ? timing.DelayIn : timing.DelayOut);
        }
        protected override void ApplyShown(Transform target) { if (target is RectTransform rect) rect.sizeDelta = shownSize; }
        protected override void ApplyHidden(Transform target) { if (target is RectTransform rect) rect.sizeDelta = hiddenSize; }
    }


    [Serializable]
    public sealed class MoveFadeTweenEffect : UITweenDefinition
    {
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -100f);
        [NonSerialized] private Vector2 shownPosition;
        [NonSerialized] private float shownAlpha;
        public override TweenEffectType Type => TweenEffectType.MoveFade;

        protected override void CaptureShownState(Transform target)
        {
            if (!(target is RectTransform rect)) throw new MissingComponentException("MoveFade requires a RectTransform.");
            shownPosition = rect.anchoredPosition;
            shownAlpha = TweenFadeMotion.GetGroup(target).alpha;
        }

        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing)
        {
            float duration = show ? timing.DurationIn : timing.DurationOut;
            return TweenFadeMotion.Create(target, ((RectTransform)target).DOAnchorPos(show ? shownPosition : shownPosition + hiddenOffset, duration),
                show, shownAlpha, duration, show ? timing.EaseIn : timing.EaseOut,
                show ? timing.DelayIn : timing.DelayOut);
        }

        protected override void ApplyShown(Transform target)
        {
            if (target is RectTransform rect) rect.anchoredPosition = shownPosition;
            TweenFadeMotion.Apply(target, true, shownAlpha);
        }

        protected override void ApplyHidden(Transform target)
        {
            if (target is RectTransform rect) rect.anchoredPosition = shownPosition + hiddenOffset;
            TweenFadeMotion.Apply(target, false, shownAlpha);
        }
    }


    [Serializable]
    public sealed class ScaleFadeTweenEffect : UITweenDefinition
    {
        [SerializeField] private Vector3 hiddenScale = Vector3.zero;
        [NonSerialized] private Vector3 shownScale;
        [NonSerialized] private float shownAlpha;
        public override TweenEffectType Type => TweenEffectType.ScaleFade;

        protected override void CaptureShownState(Transform target)
        {
            shownScale = target.localScale;
            shownAlpha = TweenFadeMotion.GetGroup(target).alpha;
        }

        protected override Tween CreateTween(Transform target, bool show, TweenTimingData timing)
        {
            float duration = show ? timing.DurationIn : timing.DurationOut;
            return TweenFadeMotion.Create(target, target.DOScale(show ? shownScale : hiddenScale, duration),
                show, shownAlpha, duration, show ? timing.EaseIn : timing.EaseOut,
                show ? timing.DelayIn : timing.DelayOut);
        }

        protected override void ApplyShown(Transform target)
        {
            target.localScale = shownScale;
            TweenFadeMotion.Apply(target, true, shownAlpha);
        }

        protected override void ApplyHidden(Transform target)
        {
            target.localScale = hiddenScale;
            TweenFadeMotion.Apply(target, false, shownAlpha);
        }
    }

}
