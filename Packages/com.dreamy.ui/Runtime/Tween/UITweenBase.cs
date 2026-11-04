using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace Dreamy.UI
{
    public abstract class UITweenBase : MonoBehaviour, ITween
    {
        [SerializeField] protected ETweenRun runType = ETweenRun.Auto;
        [SerializeField] protected TweenSettings settings;
        [SerializeField] private bool overrideEaseIn;
        [SerializeField] private Ease easeIn = Ease.OutBack;
        [SerializeField] private Ease easeOut = Ease.InBack;
        [SerializeField] private bool overrideEaseOut;
        [SerializeField] private bool overrideDurationIn;
        [SerializeField, Min(0f)] private float durationIn = 0.25f;
        [SerializeField, Min(0f)] private float durationOut = 0.2f;
        [SerializeField] private bool overrideDurationOut;
        [SerializeField] private bool overrideDelayIn;
        [SerializeField, Min(0f)] private float delayIn;
        [SerializeField] private bool overrideDelayOut;
        [SerializeField, Min(0f)] private float delayOut;

        private bool isInitialized;
        private TweenPlayback playback;
        private bool hasDelayOverride;
        private float delayInOverride;
        private float delayOutOverride;
        private bool initializationFailed;
        [System.NonSerialized] private TweenSettings inheritedSettings;

        public bool IsAutoRun => runType == ETweenRun.Auto;
        public bool IsEnabled => enabled;
        internal abstract TweenEffectType EffectType { get; }
        public Ease EaseIn => ResolveTiming().EaseIn;
        public Ease EaseOut => ResolveTiming().EaseOut;
        public float DurationIn => ResolveTiming().DurationIn;
        public float DurationOut => ResolveTiming().DurationOut;
        public float DelayIn => ResolveTiming().DelayIn;
        public float DelayOut => ResolveTiming().DelayOut;

        protected virtual void Reset()
        {
            settings = TweenPresetLibrary.Resolve(EffectType);
        }

        public UniTask Init()
        {
            if (isInitialized)
            {
                return UniTask.CompletedTask;
            }

            isInitialized = true;
            try
            {
                Setup();
                Inactive();
            }
            catch (Exception exception)
            {
                initializationFailed = true;
                Debug.LogWarning($"{GetType().Name} was skipped because it could not initialize: {exception.Message}", this);
            }
            return UniTask.CompletedTask;
        }

        public UniTask Show(CancellationToken token)
        {
            return PlaySafely(true, token);
        }

        public UniTask Hide(CancellationToken token)
        {
            return PlaySafely(false, token);
        }

        public UniTask Show() => Show(CancellationToken.None);

        public UniTask Hide() => Hide(CancellationToken.None);

        public void Kill()
        {
            Playback.Kill();
        }

        public void SetDelayOverride(float showDelay, float hideDelay)
        {
            hasDelayOverride = true;
            delayInOverride = Mathf.Max(0f, showDelay);
            delayOutOverride = Mathf.Max(0f, hideDelay);
        }

        public void ClearDelayOverride()
        {
            hasDelayOverride = false;
        }

        internal void SetInheritedSettings(TweenSettings value)
        {
            inheritedSettings = value;
        }

        protected Transform TargetTransform => transform;

        protected T ResolveTarget<T>(T serializedTarget) where T : Component
        {
            return serializedTarget != null ? serializedTarget : GetComponent<T>();
        }

        protected virtual void Setup()
        {
        }

        protected UniTask Play(Tween tween, System.Action onComplete)
        {
            if (tween == null)
            {
                return UniTask.CompletedTask;
            }

            return Playback.Play(tween, onComplete, this);
        }

        private async UniTask PlaySafely(bool show, CancellationToken token)
        {
            if (this == null || initializationFailed)
            {
                return;
            }

            if (!isInitialized)
            {
                Init();
            }

            try
            {
                await (show ? CreateShowTween() : CreateHideTween()).AttachExternalCancellation(token);
            }
            catch (OperationCanceledException)
            {
                Kill();
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"{GetType().Name} was skipped because its target is no longer valid: {exception.Message}", this);
                Kill();
            }
        }

        protected abstract UniTask CreateShowTween();

        protected abstract UniTask CreateHideTween();

        protected abstract void Active();

        protected abstract void Inactive();

        private TweenTimingData ResolveTiming()
        {
            TweenSettings preset = settings ? settings : inheritedSettings;
            TweenTimingData timing = TweenSettingsResolver.Resolve(
                preset,
                overrideEaseIn,
                easeIn,
                overrideEaseOut,
                easeOut,
                overrideDurationIn,
                durationIn,
                overrideDurationOut,
                durationOut,
                // Keep non-zero delays authored before override flags were introduced.
                overrideDelayIn || delayIn != 0f ? delayIn : preset ? preset.DelayIn : 0f,
                overrideDelayOut || delayOut != 0f ? delayOut : preset ? preset.DelayOut : 0f);
            timing = timing.WithDelays(
                hasDelayOverride ? delayInOverride : timing.DelayIn,
                hasDelayOverride ? delayOutOverride : timing.DelayOut);
            return timing;
        }

        private TweenPlayback Playback
        {
            get
            {
                if (playback == null)
                {
                    playback = new TweenPlayback();
                }

                return playback;
            }
        }
    }
}
