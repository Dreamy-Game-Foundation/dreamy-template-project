using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Assets;
using Dreamy.Core;
using UnityEngine;

namespace Dreamy.UI
{
    public sealed class PanelManager : MonoSingleton<PanelManager>
    {
        private readonly Dictionary<UILayer, Transform> layerRoots =
            new Dictionary<UILayer, Transform>();
        private readonly List<UIPanel> panels = new List<UIPanel>();
        private readonly List<UIPanel> stackPanels = new List<UIPanel>();
        private readonly Dictionary<Type, UniTaskCompletionSource<UIPanel>> creationRequests =
            new Dictionary<Type, UniTaskCompletionSource<UIPanel>>();
        private readonly Dictionary<UIPanel, UIPanel> previousPanels =
            new Dictionary<UIPanel, UIPanel>();
        private readonly SemaphoreSlim transitionLock = new SemaphoreSlim(1, 1);

        private PanelPresenterFactory presenterFactory;

        public PanelPresenterFactory PresenterFactory
        {
            get => presenterFactory;
            set
            {
                if (ReferenceEquals(presenterFactory, value)) return;
                presenterFactory = value;
                foreach (var panel in panels.ToArray())
                    if (panel != null) panel.ReleasePresenter();
            }
        }

        public UIPanel LastPanel
        {
            get
            {
                PruneDestroyedPanels(stackPanels);
                return stackPanels.Count > 0 ? stackPanels[stackPanels.Count - 1] : null;
            }
        }

        public Type LastPanelType => LastPanel != null ? LastPanel.GetType() : null;

        protected override bool IsPersistent => false;

        protected override void Awake()
        {
            base.Awake();

            RefreshLayerRoots(true);
        }

        private void OnValidate()
        {
            RefreshLayerRoots(false);
        }

        private void Start()
        {
            InitializeChildPanels().Forget();
        }

        private async UniTaskVoid InitializeChildPanels()
        {
            List<UIPanel> childPanels = GetComponentsInChildren<UIPanel>(true).ToList();
            foreach (UIPanel panel in childPanels)
            {
                await panel.Init();
            }

            foreach (UIPanel panel in childPanels)
            {
                await panel.PostInit();
            }

            foreach (UIPanel panel in childPanels)
            {
                if (panel.ShowOnStart)
                {
                    panel.Show().Forget();
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TryCloseCurrentPanel();
            }
        }

        public UniTask<TPanel> Create<TPanel>(string address) where TPanel : UIPanel
            => CreatePanel<TPanel>(() => AssetLoader.LoadAsync<GameObject>(address), address);

        public UniTask<TPanel> Create<TPanel>(GameObject prefab) where TPanel : UIPanel
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            return CreatePanel<TPanel>(() => UniTask.FromResult(prefab), prefab.name);
        }

        private async UniTask<TPanel> CreatePanel<TPanel>(Func<UniTask<GameObject>> load, string source)
            where TPanel : UIPanel
        {
            Type panelType = typeof(TPanel);
            // Init registers panels before PostInit finishes: wait for the request first.
            if (creationRequests.TryGetValue(panelType, out var pendingRequest))
                return (TPanel)await pendingRequest.Task;
            if (TryGet(out TPanel existingPanel)) return existingPanel;

            var completionSource = new UniTaskCompletionSource<UIPanel>();
            creationRequests.Add(panelType, completionSource);
            GameObject instance = null;
            try
            {
                var prefab = await load();
                if (this == null) throw new OperationCanceledException("Panel manager was destroyed.");
                instance = Instantiate(prefab, transform);
                TPanel panel = instance.GetComponent<TPanel>();
                if (panel == null)
                    throw new MissingComponentException(
                        $"UI prefab '{source}' does not contain {typeof(TPanel).Name}.");

                instance.transform.SetParent(GetLayerRoot(panel.Layer), false);
                await panel.Init();
                await panel.PostInit();
                if (this == null || panel == null)
                    throw new OperationCanceledException("Panel owner was destroyed during initialization.");
                completionSource.TrySetResult(panel);
                return panel;
            }
            catch (Exception exception)
            {
                if (instance != null)
                {
                    var panel = instance.GetComponent<UIPanel>();
                    if (panel != null) Unregister(panel);
                    Destroy(instance);
                }
                completionSource.TrySetException(exception);
                throw;
            }
            finally { creationRequests.Remove(panelType); }
        }

        public async UniTask<TPanel> Show<TPanel>(string address) where TPanel : UIPanel
        {
            TPanel panel = await Create<TPanel>(address);

            await panel.Show();
            return panel;
        }

        public async UniTask<TPanel> Show<TPanel>(GameObject prefab) where TPanel : UIPanel
        {
            TPanel panel = await Create<TPanel>(prefab);
            await panel.Show();
            return panel;
        }

        public async UniTask Close<TPanel>() where TPanel : UIPanel
        {
            if (!TryGet(out TPanel panel))
            {
                return;
            }

            await panel.Hide();
        }

        public TPanel Get<TPanel>() where TPanel : UIPanel
        {
            if (TryGet(out TPanel panel))
            {
                return panel;
            }

            Debug.LogWarning($"[PANEL] Not found panel {typeof(TPanel).Name}.");
            return null;
        }

        public bool TryGet<TPanel>(out TPanel result) where TPanel : UIPanel
        {
            PruneDestroyedPanels(panels);
            for (int i = panels.Count - 1; i >= 0; i--)
            {
                UIPanel panel = panels[i];
                if (panel != null && panel.GetType() == typeof(TPanel))
                {
                    result = (TPanel)panel;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public async UniTask<TPanel> Transition<TPanel>(string address) where TPanel : UIPanel
        {
            await transitionLock.WaitAsync();
            try
            {
                UIPanel lastPanel = LastPanel;
                if (lastPanel != null)
                {
                    lastPanel.SetInteractable(false);
                }

                TPanel newPanel = await Create<TPanel>(address);
                if (ReferenceEquals(lastPanel, newPanel))
                {
                    newPanel.SetInteractable(true);
                    return newPanel;
                }

                if (lastPanel == null)
                {
                    await newPanel.Show();
                    return newPanel;
                }

                previousPanels[newPanel] = lastPanel;
                try
                {
                    await UniTask.WhenAll(newPanel.Show(), lastPanel.HideTween());
                    return newPanel;
                }
                catch
                {
                    previousPanels.Remove(newPanel);
                    try
                    {
                        await lastPanel.ShowTween();
                    }
                    finally
                    {
                        lastPanel.SetInteractable(true);
                    }

                    throw;
                }
            }
            finally
            {
                transitionLock.Release();
            }
        }

        internal UniTask RestorePreviousPanelVisual(UIPanel panel)
        {
            return previousPanels.TryGetValue(panel, out UIPanel previousPanel) && previousPanel != null
                ? previousPanel.ShowTween()
                : UniTask.CompletedTask;
        }

        internal void CompletePanelHide(UIPanel panel)
        {
            if (previousPanels.Remove(panel, out UIPanel previousPanel) && previousPanel != null)
            {
                previousPanel.SetInteractable(true);
            }
        }

        internal void CancelPanelHide(UIPanel panel)
        {
            if (previousPanels.TryGetValue(panel, out UIPanel previousPanel) && previousPanel != null)
            {
                previousPanel.HideTween().Forget();
            }
        }

        public void Register(UIPanel panel)
        {
            if (panel != null && !panels.Contains(panel))
            {
                panel.transform.SetParent(GetLayerRoot(panel.Layer), false);
                panels.Add(panel);
            }
        }

        public void Unregister(UIPanel panel)
        {
            panels.Remove(panel);
            stackPanels.Remove(panel);
            CompletePanelHide(panel);
            previousPanels.Remove(panel);

            List<UIPanel> orphanedKeys = new List<UIPanel>();
            foreach (KeyValuePair<UIPanel, UIPanel> pair in previousPanels)
            {
                if (pair.Value == panel || pair.Key == null || pair.Value == null)
                {
                    orphanedKeys.Add(pair.Key);
                }
            }

            foreach (UIPanel key in orphanedKeys)
            {
                previousPanels.Remove(key);
            }
        }

        public void MarkShown(UIPanel panel)
        {
            stackPanels.Remove(panel);
            stackPanels.Add(panel);
        }

        public void MarkHidden(UIPanel panel)
        {
            stackPanels.Remove(panel);
        }

        public Transform GetLayerRoot(UILayer layer)
        {
            return layerRoots.TryGetValue(layer, out Transform root)
                ? root
                : transform;
        }

        private void TryCloseCurrentPanel()
        {
            UIPanel panel = LastPanel;
            if (panel == null || !panel.CanBack)
            {
                return;
            }

            panel.Hide().Forget();
        }

        private static void PruneDestroyedPanels(List<UIPanel> source)
        {
            source.RemoveAll(panel => panel == null);
        }

        [ContextMenu("Create Missing Layer Roots")]
        private void CreateMissingLayerRoots()
        {
            RefreshLayerRoots(true);
        }

        private void RefreshLayerRoots(bool createMissing)
        {
            layerRoots.Clear();
            UILayerRoot[] roots = GetComponentsInChildren<UILayerRoot>(true);
            foreach (UILayerRoot root in roots)
            {
                if (!layerRoots.TryAdd(root.Layer, root.transform))
                {
                    Debug.LogWarning(
                        $"[PANEL] Multiple roots found for {root.Layer}. " +
                        "The first root will be used.",
                        root);
                }
            }

            if (!createMissing)
            {
                return;
            }

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                if (layerRoots.ContainsKey(layer))
                {
                    continue;
                }

                GameObject rootObject = new GameObject(
                    layer.ToString(),
                    typeof(RectTransform),
                    typeof(UILayerRoot));
                RectTransform rootTransform = (RectTransform)rootObject.transform;
                rootTransform.SetParent(transform, false);
                rootTransform.anchorMin = Vector2.zero;
                rootTransform.anchorMax = Vector2.one;
                rootTransform.offsetMin = Vector2.zero;
                rootTransform.offsetMax = Vector2.zero;
                UILayerRoot layerRoot = rootObject.GetComponent<UILayerRoot>();
                layerRoot.SetLayer(layer);
                layerRoots.Add(layer, rootTransform);
            }

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                if (layerRoots.TryGetValue(layer, out Transform root))
                {
                    root.SetSiblingIndex((int)layer);
                }
            }
        }
    }
}
