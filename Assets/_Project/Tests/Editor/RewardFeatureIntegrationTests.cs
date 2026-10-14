using System;
using System.IO;
using System.Linq;
using Dreamy.DailyReward;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Economy.Rewards;
using Dreamy.Missions;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Dreamy.Template.Tests
{
    public sealed class RewardFeatureIntegrationTests
    {
        private sealed class Clock : IRewardClock
        {
            public DateTime UtcNow { get; set; } = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);
        }

        private string directory;
        private DatasaveService saves;
        private DatasaveResourceWallet wallet;
        private IRewardDeliveryGateway gateway;
        private Clock clock;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "dreamy-template-rewards-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            clock = new Clock();
            ReloadOwners();
        }

        [TearDown]
        public void TearDown() => Directory.Delete(directory, recursive: true);

        private void ReloadOwners()
        {
            saves = new DatasaveService(new DatasaveOptions { DirectoryName = directory });
            wallet = new DatasaveResourceWallet(saves);
            gateway = new RewardDeliveryGateway(new IRewardDestination[] { wallet });
        }

        private static T Config<T>(string key) where T : ConfigBase
        {
            var asset = Resources.Load<TextAsset>("DataConfig/" + key);
            Assert.That(asset, Is.Not.Null);
            var config = JsonConvert.DeserializeObject<T>(asset.text);
            config.Initialize(key);
            return config;
        }

        private DailyRewardModel Daily() => new(Config<DailyRewardScheduleConfig>("dailyRewardSchedule"),
            saves, gateway, clock, DailyRewardInstaller.DefaultSaveKey);

        private MissionModel Missions() => new(Config<MissionCatalogConfig>("missionCatalog"), saves, gateway);

        [Test]
        public void Daily_StopSchedule_ClaimsSevenDaysOnce_AcrossOwnerRecreation()
        {
            var config = Config<DailyRewardScheduleConfig>("dailyRewardSchedule");
            Assert.That(config.CycleMode, Is.EqualTo(DailyRewardCycleMode.StopAfterLastDay));
            Assert.That(config.ResetProgressAfterMissedDay, Is.True);
            for (int day = 0; day < config.Rewards.Count; day++)
            {
                var daily = Daily();
                Assert.That(daily.Claim().IsSuccess, Is.True);
                Assert.That(daily.Claim().Status, Is.EqualTo(DailyRewardClaimStatus.NotAvailable)
                    .Or.EqualTo(DailyRewardClaimStatus.Completed));
                clock.UtcNow = clock.UtcNow.AddDays(1);
                ReloadOwners();
            }
            Assert.That(Daily().Claim().Status, Is.EqualTo(DailyRewardClaimStatus.Completed));
            Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(1400));
            Assert.That(wallet.GetBalance(new ResourceId("currency.gem")), Is.EqualTo(10));
        }

        [Test]
        public void Daily_MissedDay_ResetsToFirstReward_WithoutReusingItsReceipt()
        {
            Assert.That(Daily().Claim().IsSuccess, Is.True);
            clock.UtcNow = clock.UtcNow.AddDays(2);
            ReloadOwners();
            var daily = Daily();
            Assert.That(daily.GetState().Rewards[0].Status, Is.EqualTo(DailyRewardItemStatus.Claimable));
            Assert.That(daily.Claim().IsSuccess, Is.True);
            Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(200));
        }

        [Test]
        public void Missions_PermanentProgressAndClaim_ShareDailyAndShopWallet_AcrossReload()
        {
            Assert.That(Config<MissionCatalogConfig>("missionCatalog").Period.Period,
                Is.EqualTo(MissionPeriod.Permanent));
            Assert.That(Daily().Claim().IsSuccess, Is.True);
            var missions = Missions();
            Assert.That(missions.ReportProgress("stage.win", "fixture-win", 3), Is.EqualTo(MissionProgressStatus.Updated));
            Assert.That(missions.Claim("win-3-stages"), Is.EqualTo(MissionClaimStatus.Claimed));
            Assert.That(wallet.TryExchange(new ResourceExchangeRequest("fixture-shop",
                new ResourceAmount(new ResourceId("currency.coin"), 50),
                new[] { new ResourceAmount(new ResourceId("currency.gem"), 1) })), Is.True);
            clock.UtcNow = clock.UtcNow.AddDays(30);
            ReloadOwners();
            missions = Missions();
            Assert.That(missions.ReportProgress("stage.win", "fixture-win", 3), Is.EqualTo(MissionProgressStatus.AlreadyProcessed));
            Assert.That(missions.GetState().Single(x => x.Definition.Id == "win-3-stages").IsClaimed, Is.True);
            Assert.That(missions.Claim("win-3-stages"), Is.EqualTo(MissionClaimStatus.AlreadyClaimed));
            Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(150));
            Assert.That(wallet.GetBalance(new ResourceId("currency.gem")), Is.EqualTo(1));
        }

        [Test]
        public void CurrencyOnlyGateway_RejectsMixedOwnerBundle_BeforeAnyGrant()
        {
            var bundle = new ResolvedRewardBundle(1, "template-fixture", new[]
            {
                new ResolvedRewardEntry("coins", "currency", "currency.coin", 100),
                new ResolvedRewardEntry("item", "item", "fixture.item", 1)
            });
            Assert.That(gateway.Validate("fixture-request", "fixture-fingerprint", bundle).Reason,
                Is.EqualTo(RewardDeliveryReason.UnknownKind));
            ReloadOwners();
            Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.Zero);
        }
    }
}
