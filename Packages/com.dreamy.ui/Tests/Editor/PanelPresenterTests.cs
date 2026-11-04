using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dreamy.UI.Tests.Editor
{
    public sealed class PresenterTestPanel : UIPanel
    {
        public override bool CanBack => true;
        public override bool CanCache => true;
    }

    public sealed class PresenterTestWidget : MonoBehaviour { }

    public sealed class PendingPresenterTestPanel : UIPanel
    {
        public static UniTaskCompletionSource Initialization;
        public override bool CanBack => true;
        public override bool CanCache => true;
        public override async UniTask PostInit()
        {
            await base.PostInit();
            await Initialization.Task;
        }
    }

    public sealed class PanelPresenterTests
    {
        private GameObject root;
        private PanelManager manager;
        private PresenterTestPanel panel;
        private int creates, shows, disposals;
        private bool failShow;

        private sealed class Presenter : IPanelPresenter
        {
            private readonly PanelPresenterTests owner;
            public Presenter(PanelPresenterTests owner) => this.owner = owner;
            public void Show()
            {
                Assert.IsTrue(owner.panel.gameObject.activeInHierarchy, "Presenter must start after panel activation.");
                owner.shows++;
                if (owner.failShow) throw new InvalidOperationException("show failure");
            }
            public void Dispose() => owner.disposals++;
        }

        private sealed class WidgetPresenter : ITickedPanelPresenter
        {
            public int Shows, Disposals, Ticks;
            public void Show() => Shows++;
            public void Tick() => Ticks++;
            public void Dispose() => Disposals++;
        }

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Presenter manager");
            manager = root.AddComponent<PanelManager>();
            panel = new GameObject("Cached view").AddComponent<PresenterTestPanel>();
            panel.transform.SetParent(root.transform);
            panel.Init().GetAwaiter().GetResult();
            panel.PostInit().GetAwaiter().GetResult();
            var factory = new PanelPresenterFactory();
            factory.Register<PresenterTestPanel>(_ => { creates++; return new Presenter(this); });
            manager.PresenterFactory = factory;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            PendingPresenterTestPanel.Initialization = null;
        }

        [Test]
        public void ManagerShow_CachedReopen_CreatesOncePerOpening()
        {
            manager.Show<PresenterTestPanel>("already-created").GetAwaiter().GetResult();
            manager.Show<PresenterTestPanel>("already-created").GetAwaiter().GetResult();
            Assert.AreEqual(1, creates);
            Assert.AreEqual(1, shows);
            panel.Hide().GetAwaiter().GetResult();
            Assert.AreEqual(1, disposals);
            panel.Show().GetAwaiter().GetResult();
            Assert.AreEqual(2, creates);
            Object.DestroyImmediate(panel.gameObject);
            Assert.AreEqual(2, disposals);
        }

        [Test]
        public void FailedShow_ReleasesPresenter_AndCanRetry()
        {
            failShow = true;
            Assert.Throws<InvalidOperationException>(() => panel.Show().GetAwaiter().GetResult());
            Assert.AreEqual(1, disposals);
            Assert.IsNull(manager.LastPanel);
            failShow = false;
            panel.Show().GetAwaiter().GetResult();
            Assert.AreEqual(2, creates);
            Assert.AreSame(panel, manager.LastPanel);
        }

        [Test]
        public void DisableAndFactoryTeardown_ReleaseExactlyOnce()
        {
            panel.Show().GetAwaiter().GetResult();
            panel.gameObject.SetActive(false);
            Assert.AreEqual(1, disposals);
            panel.Show().GetAwaiter().GetResult();
            manager.PresenterFactory = null;
            Assert.AreEqual(2, disposals);
            panel.Hide().GetAwaiter().GetResult();
            Assert.AreEqual(2, disposals);
        }

        [Test]
        public void Transition_VisualPreviousPanel_KeepsPresenterUntilClosed()
        {
            panel.Show().GetAwaiter().GetResult();
            var second = new GameObject("Second view").AddComponent<PendingPresenterTestPanel>();
            second.transform.SetParent(root.transform);
            PendingPresenterTestPanel.Initialization = new UniTaskCompletionSource();
            PendingPresenterTestPanel.Initialization.TrySetResult();
            second.Init().GetAwaiter().GetResult();
            second.PostInit().GetAwaiter().GetResult();
            manager.Transition<PendingPresenterTestPanel>("already-created").GetAwaiter().GetResult();
            Assert.AreEqual(0, disposals);
            second.Hide().GetAwaiter().GetResult();
            Assert.AreSame(panel, manager.LastPanel);
            Assert.AreEqual(0, disposals);
            panel.Hide().GetAwaiter().GetResult();
            Assert.AreEqual(1, disposals);
        }

        [Test]
        public void NonPanelHost_ReopenDisableDestroy_UsesTheSharedLifecycle()
        {
            var widget = new GameObject("Widget").AddComponent<PresenterTestWidget>();
            widget.transform.SetParent(root.transform);
            var factory = new PanelPresenterFactory();
            WidgetPresenter current = null;
            factory.Register<PresenterTestWidget>(_ => current = new WidgetPresenter());
            var host = widget.gameObject.AddComponent<PresenterViewHost>();
            host.Initialize(factory, widget);
            Assert.AreEqual(1, current.Shows);
            var first = current;
            widget.gameObject.SetActive(false);
            Assert.AreEqual(1, first.Disposals);
            widget.gameObject.SetActive(true);
            Assert.AreNotSame(first, current);
            Assert.AreEqual(1, current.Shows);
            host.Release();
            Assert.AreEqual(1, current.Disposals);
            Object.DestroyImmediate(widget.gameObject);
            Assert.AreEqual(1, current.Disposals);
        }

        [Test]
        public void ConcurrentCreate_WaitsForPostInit_InsteadOfUsingRegisteredPartialPanel()
        {
            var prefab = new GameObject("Pending prefab");
            prefab.AddComponent<PendingPresenterTestPanel>();
            PendingPresenterTestPanel.Initialization = new UniTaskCompletionSource();
            try
            {
                var first = manager.Create<PendingPresenterTestPanel>(prefab);
                var second = manager.Create<PendingPresenterTestPanel>(prefab);
                Assert.AreEqual(UniTaskStatus.Pending, first.Status);
                Assert.AreEqual(UniTaskStatus.Pending, second.Status);
                PendingPresenterTestPanel.Initialization.TrySetResult();
                Assert.AreSame(first.GetAwaiter().GetResult(), second.GetAwaiter().GetResult());
            }
            finally { Object.DestroyImmediate(prefab); }
        }
    }
}
