#if !DISABLE_DREAMY_DEBUGGER
using System;
using System.ComponentModel;
using Dreamy.Datasave;
using Dreamy.Debugger.Options;
using Dreamy.Debugger.Presentation;
using Dreamy.Economy;
using UnityEngine;

namespace Dreamy.Template.Debugging
{
    public sealed class TemplateCheats
    {
        private readonly IResourceWallet wallet;
        private readonly IResourceBalanceProvider balances;
        private readonly IDatasaveService saves;
        public TemplateCheats(IResourceWallet wallet, IResourceBalanceProvider balances,
            IDatasaveService saves)
        {
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.balances = balances ?? throw new ArgumentNullException(nameof(balances));
            this.saves = saves ?? throw new ArgumentNullException(nameof(saves));
        }
        [Category("Economy"), ResourceCheat("template.coins", "Coins", CheatCardRole.Balance)]
        public long Coins => Balance("currency.coin");
        [Category("Economy"), ResourceCheat("template.coins", "Coins", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(100)]
        public long CoinAmount { get; set; } = 1000;
        [Category("Economy"), ResourceCheat("template.coins", "Coins", CheatCardRole.Action)]
        public void AddCoins() => Grant("currency.coin", CoinAmount);
        [Category("Economy"), ResourceCheat("template.gems", "Gems", CheatCardRole.Balance)]
        public long Gems => Balance("currency.gem");
        [Category("Economy"), ResourceCheat("template.gems", "Gems", CheatCardRole.Amount)]
        [NumberRange(1, 1000000), Increment(10)]
        public long GemAmount { get; set; } = 100;
        [Category("Economy"), ResourceCheat("template.gems", "Gems", CheatCardRole.Action)]
        public void AddGems() => Grant("currency.gem", GemAmount);

        [Category("Shop ownership"), OwnershipCheat("template.remove-ads", "Remove Ads", CheatCardRole.Owned)]
        public bool RemoveAds => Balance("entitlement.remove-ads") > 0;
        [Category("Shop ownership"), OwnershipCheat("template.remove-ads", "Remove Ads", CheatCardRole.Action)]
        public void GrantRemoveAds() { if (!RemoveAds) Grant("entitlement.remove-ads", 1); }

        [Category("Shop ownership"), OwnershipCheat("template.vip", "Lifetime VIP", CheatCardRole.Owned)]
        public bool LifetimeVip => Balance("entitlement.vip.lifetime") > 0;
        [Category("Shop ownership"), OwnershipCheat("template.vip", "Lifetime VIP", CheatCardRole.Action)]
        public void GrantLifetimeVip() { if (!LifetimeVip) Grant("entitlement.vip.lifetime", 1); }

        [Category("Shop ownership"), OwnershipCheat("template.starter-fund", "Starter Fund", CheatCardRole.Owned)]
        public bool StarterFund => Balance("entitlement.fund.starter") > 0;
        [Category("Shop ownership"), OwnershipCheat("template.starter-fund", "Starter Fund", CheatCardRole.Action)]
        public void GrantStarterFund() { if (!StarterFund) Grant("entitlement.fund.starter", 1); }

        [Category("Shop ownership"), OwnershipCheat("template.season-pass", "Season 1 Premium Pass", CheatCardRole.Owned)]
        public bool SeasonPass => Balance("entitlement.battle-pass.season-1.premium") > 0;
        [Category("Shop ownership"), OwnershipCheat("template.season-pass", "Season 1 Premium Pass", CheatCardRole.Action)]
        public void GrantSeasonPass() { if (!SeasonPass) Grant("entitlement.battle-pass.season-1.premium", 1); }

        [Category("Runtime"), CheatId("template.bootstrap"), DisplayName("Bootstrap state")]
        public string Bootstrap => GameInstaller.State.ToString();
        [Category("Runtime"), CheatId("template.time-scale"), NumberRange(.1, 3), Increment(.1)]
        public float TimeScale { get => Time.timeScale; set => Time.timeScale = Mathf.Clamp(value, .1f, 3f); }
        [Category("Runtime"), CheatId("template.fps"), NumberRange(15, 120), Increment(15)]
        public int TargetFps { get => Application.targetFrameRate; set => Application.targetFrameRate = Mathf.Clamp(value, 15, 120); }
        [Category("Save"), CheatId("template.save"), DisplayName("Save all now"), CheatConfirmation]
        public void SaveAll() => saves.SaveAll();
        private long Balance(string id) => balances.GetBalance(new ResourceId(id));
        private void Grant(string id, long amount)
        {
            if (amount < 1 || amount > 1000000) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!wallet.TryGrant(new ResourceGrantRequest("debug-" + Guid.NewGuid().ToString("N"),
                new ResourceAmount(new ResourceId(id), amount))))
                throw new InvalidOperationException("Debug wallet grant was rejected.");
        }
    }
}
#endif
