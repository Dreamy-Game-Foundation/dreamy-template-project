using System;
using System.Collections.Generic;
using Dreamy.DailyReward;
using Dreamy.Missions;
using Dreamy.UI;
using NUnit.Framework;

namespace Dreamy.Template.Tests
{
    public sealed class RewardPresenterLifecycleTests
    {
        private sealed class DailyView : IDailyRewardView
        {
            public event Action ClaimRequested;
            public event Action CloseRequested;
            public void Render(DailyRewardViewState state) { }
            public void SetClaimInteractable(bool value) { }
            public void ShowClaimResult(DailyRewardClaimResult result) { }
            public void Close() { }
            public void Claim() => ClaimRequested?.Invoke();
            public void RequestClose() => CloseRequested?.Invoke();
        }

        private sealed class DailyService : IDailyRewardService
        {
            public int Claims;
            public DailyRewardViewState GetState() => new(Array.Empty<DailyRewardItemViewState>(), true, DateTime.MinValue);
            public DailyRewardClaimResult Claim() { Claims++; return DailyRewardClaimResult.NotAvailable(); }
        }

        private sealed class MissionView : IMissionView
        {
            public event Action<string> ClaimRequested;
            public event Action CloseRequested;
            public void Render(IReadOnlyList<MissionState> state) { }
            public void ShowClaimResult(MissionClaimStatus result) { }
            public void Close() { }
            public void Claim() => ClaimRequested?.Invoke("fixture");
            public void RequestClose() => CloseRequested?.Invoke();
        }

        private sealed class MissionService : IMissionService
        {
            private Action changed;
            public int Subscriptions => changed?.GetInvocationList().Length ?? 0;
            public int Claims;
            public event Action StateChanged { add => changed += value; remove => changed -= value; }
            public string WindowId => "permanent";
            public IReadOnlyList<MissionState> GetState() => Array.Empty<MissionState>();
            public MissionClaimStatus Claim(string id) { Claims++; return MissionClaimStatus.NotComplete; }
            public MissionProgressStatus ReportProgress(string key, string id, long amount = 1) => MissionProgressStatus.NoMatchingMission;
            public MissionProgressStatus ReportProgress(string key, string id, string ns, string window,
                long amount = 1, string distinctValue = null) => MissionProgressStatus.NoMatchingMission;
            public MissionRefreshStatus Refresh() => MissionRefreshStatus.Unchanged;
            public MissionRerollStatus Reroll(string id) => MissionRerollStatus.Ineligible;
            public MissionClaimStatus RecoverClaims() => MissionClaimStatus.AlreadyClaimed;
        }

        [Test]
        public void DailyFactory_RepeatedOpenClose_HasOneHandler_AndReleasesOldPresenter()
        {
            var view = new DailyView();
            var service = new DailyService();
            var factory = new PanelPresenterFactory();
            factory.Register<DailyView>(v => new DailyRewardPresenter(service, v));
            using var host = new PanelPresenterHost(factory, view);
            for (int i = 0; i < 100; i++)
            {
                host.Show();
                host.Show();
                view.Claim();
                Assert.That(service.Claims, Is.EqualTo(i + 1));
                view.RequestClose();
                host.Dispose();
                view.Claim();
                Assert.That(service.Claims, Is.EqualTo(i + 1));
            }
        }

        [Test]
        public void MissionFactory_RepeatedOpenClose_ReleasesServiceAndViewHandlers()
        {
            var view = new MissionView();
            var service = new MissionService();
            var factory = new PanelPresenterFactory();
            factory.Register<MissionView>(v => new MissionPresenter(service, v));
            using var host = new PanelPresenterHost(factory, view);
            for (int i = 0; i < 100; i++)
            {
                host.Show();
                host.Show();
                Assert.That(service.Subscriptions, Is.EqualTo(1));
                view.Claim();
                Assert.That(service.Claims, Is.EqualTo(i + 1));
                view.RequestClose();
                host.Dispose();
                Assert.That(service.Subscriptions, Is.Zero);
                view.Claim();
                Assert.That(service.Claims, Is.EqualTo(i + 1));
            }
        }
    }
}
