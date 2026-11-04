using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.UI
{
    [DisallowMultipleComponent]
    public sealed class UITweenPlayer : MonoBehaviour
    {
        [SerializeField] private TweenCollectionMode collectionMode = TweenCollectionMode.Auto;
        [SerializeField] private List<TweenTargetGroup> manualTargets =
            new List<TweenTargetGroup>();

        private readonly List<ITween> cachedTweens = new List<ITween>();
        private bool cacheDirty = true;
        private bool initialized;

        public TweenCollectionMode CollectionMode => collectionMode;
        public IReadOnlyList<ITween> Tweens => cachedTweens;

        private void Awake()
        {
            RebuildCache();
        }

        private void OnEnable()
        {
            cacheDirty = true;
        }

        private void OnDisable()
        {
            Kill();
        }

        private void OnDestroy()
        {
            Kill();
            cachedTweens.Clear();
        }

        private void OnTransformChildrenChanged()
        {
            cacheDirty = true;
        }

        public UniTask Init()
        {
            EnsureCache();
            initialized = true;
            InitializeCachedTweens();
            return UniTask.CompletedTask;
        }

        public UniTask ShowTween(CancellationToken token)
        {
            return Play(true, token);
        }

        public UniTask HideTween(CancellationToken token)
        {
            return Play(false, token);
        }

        public void Kill()
        {
            PruneCache();
            foreach (ITween tween in cachedTweens)
            {
                tween.Kill();
            }
        }

        public void RebuildCache()
        {
            foreach (ITween tween in cachedTweens)
            {
                if (tween is UnityEngine.Object unityObject && unityObject == null) continue;
                tween.Kill();
            }
            cachedTweens.Clear();
            if (collectionMode == TweenCollectionMode.Auto)
            {
                HashSet<UITweenBase> unique = new HashSet<UITweenBase>();
                UITweenBase[] discovered = GetComponentsInChildren<UITweenBase>(true);
                foreach (UITweenBase tween in discovered)
                {
                    if (IsOwnedByThisPlayer(tween) && unique.Add(tween))
                    {
                        tween.SetInheritedSettings(ResolvePreset(tween.EffectType));
                        cachedTweens.Add(tween);
                    }
                }
            }
            else
            {
                foreach (TweenTargetGroup group in manualTargets)
                {
                    group?.CollectTweens(cachedTweens, this);
                }
            }

            cacheDirty = false;
        }

        private async UniTask Play(bool show, CancellationToken token)
        {
            EnsureCache();
            try
            {
                List<UniTask> tasks = new List<UniTask>();
                foreach (ITween tween in cachedTweens)
                {
                    if (!IsPlayable(tween)) continue;

                    tasks.Add(show ? tween.Show(token) : tween.Hide(token));
                }

                await UniTask.WhenAll(tasks).AttachExternalCancellation(token);
            }
            catch (OperationCanceledException)
            {
                Kill();
                throw;
            }
            finally
            {
                PruneCache();
            }
        }

        private void EnsureCache()
        {
            if (cacheDirty || !initialized)
            {
                RebuildCache();
            }

            PruneCache();
            if (initialized)
            {
                InitializeCachedTweens();
            }
        }

        private void InitializeCachedTweens()
        {
            foreach (ITween tween in cachedTweens)
            {
                tween.Init();
            }
        }

        private void PruneCache()
        {
            cachedTweens.RemoveAll(tween => tween == null ||
                (tween is UnityEngine.Object unityObject && unityObject == null) ||
                (collectionMode == TweenCollectionMode.Auto && tween is UITweenBase component &&
                    !IsOwnedByThisPlayer(component)));
        }

        private bool IsOwnedByThisPlayer(UITweenBase tween)
        {
            if (tween == null) return false;

            return tween.GetComponentInParent<UITweenPlayer>(true) == this;
        }

        private static TweenSettings ResolvePreset(TweenEffectType type)
        {
            return TweenPresetLibrary.Resolve(type);
        }

        private bool IsPlayable(ITween tween)
        {
            return tween.IsEnabled &&
                (collectionMode == TweenCollectionMode.Manual || tween.IsAutoRun);
        }

    }
}
