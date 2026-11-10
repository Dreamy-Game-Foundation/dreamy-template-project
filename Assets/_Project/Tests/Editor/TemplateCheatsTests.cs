#if !DISABLE_DREAMY_DEBUGGER
using System;
using System.IO;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Template.Debugging;
using NUnit.Framework;

namespace Dreamy.Template.Tests
{
    public sealed class TemplateCheatsTests
    {
        [Test]
        public void GrantsPersistAndOwnershipDoesNotStack()
        {
            var folder = Path.Combine(Path.GetTempPath(), "template-cheats-" + Guid.NewGuid().ToString("N"));
            try
            {
                var saves = new DatasaveService(new DatasaveOptions { DirectoryName = folder });
                var wallet = new DatasaveResourceWallet(saves);
                {
                    var options = new TemplateCheats(wallet, wallet, saves);
                    options.CoinAmount = 123;
                    options.AddCoins(); options.AddCoins();
                    options.GemAmount = 17; options.AddGems();
                    options.GrantRemoveAds(); options.GrantRemoveAds();
                    options.GrantLifetimeVip(); options.GrantStarterFund(); options.GrantSeasonPass();
                    Assert.That(options.Coins, Is.EqualTo(246));
                    Assert.That(options.Gems, Is.EqualTo(17));
                    Assert.That(wallet.GetBalance(new ResourceId("entitlement.remove-ads")), Is.EqualTo(1));
                    Assert.That(wallet.GetBalance(new ResourceId("entitlement.vip.lifetime")), Is.EqualTo(1));
                    Assert.That(wallet.GetBalance(new ResourceId("entitlement.fund.starter")), Is.EqualTo(1));
                    Assert.That(wallet.GetBalance(new ResourceId("entitlement.battle-pass.season-1.premium")), Is.EqualTo(1));
                    options.CoinAmount = -1;
                    Assert.Throws<ArgumentOutOfRangeException>(() => options.AddCoins());
                    options.GemAmount = 1000001;
                    Assert.Throws<ArgumentOutOfRangeException>(() => options.AddGems());
                }
                var restored = new DatasaveResourceWallet(new DatasaveService(new DatasaveOptions { DirectoryName = folder }));
                    Assert.That(restored.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(246));
            }
            finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
        }
    }
}
#endif
