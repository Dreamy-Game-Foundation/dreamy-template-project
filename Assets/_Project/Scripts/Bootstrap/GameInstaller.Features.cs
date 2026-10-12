using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Core;
using Dreamy.Economy;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Settings;
using Dreamy.Shop;
using Dreamy.Template.Feedback;
using Dreamy.UI;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed partial class GameInstaller
    {
        private readonly List<EntitlementEffectBinding> entitlementBindings = new();

        private void InstallFeatures()
        {
            InstallSettings();
            InstallShop();
        }

        private void InstallSettings()
        {
            if (!ServiceLocator.TryGet<ISettingsPlatformGateway>(out var settingsGateway))
            {
                settingsGateway = new SettingsPlatformGateway();
                RegisterService<ISettingsPlatformGateway>(settingsGateway, ownsInstance: true);
            }
            if (!ServiceLocator.TryGet<ISettingsHapticsGateway>(out var hapticsGateway))
            {
                hapticsGateway = new PlayerPrefsSettingsHapticsGateway(
                    MobileHaptics.SetEnabled, () => MobileHaptics.IsSupported);
                RegisterService<ISettingsHapticsGateway>(hapticsGateway, ownsInstance: true);
            }
            ServiceLocator.TryGet<ISettingsReviewGateway>(out var reviewGateway);
            TrackFeatureInstallation<ISettingsService>(() =>
                SettingsFeatureInstaller.Install(presenterFactory, ServiceLocator.Get<IAudioService>(),
                    () => OpenRateUsAsync().Forget(), settingsGateway, hapticsGateway, reviewGateway));
        }

        private void InstallShop()
        {
            IShopPurchaseGateway purchaseGateway;
            // Local IAP simulation belongs to the Editor demo only.
            if (!ServiceLocator.TryGet(out purchaseGateway))
            {
                purchaseGateway = new SimulatedShopPurchaseGateway();
                RegisterService<IShopPurchaseGateway>(purchaseGateway, ownsInstance: true);
            }
            ServiceLocator.TryGet(out purchaseGateway);
            TrackFeatureInstallation<IShopService>(() =>
                ShopFeatureInstaller.Install(presenterFactory, dataConfig.GetTable<ShopCatalogConfig>(),
                    wallet, purchaseGateway, (IResourceBalanceProvider)wallet));
            InstallEntitlementEffects();
        }

        private void InstallEntitlementEffects()
        {
            BindEntitlement(EntitlementIds.RemoveAds, ApplyNoAdsDemo);
            BindEntitlement(EntitlementIds.LifetimeVip, ApplyLifetimeVipDemo);
            BindEntitlement(EntitlementIds.StarterFund, UnlockStarterFundDemo);
            BindEntitlement(EntitlementIds.Season1PremiumPass, UnlockSeason1PremiumPassDemo);
        }

        private void BindEntitlement(ResourceId id, Action effect)
        {
            entitlementBindings.Add(new EntitlementEffectBinding(
                walletBalanceSource, id, effect, exception => Debug.LogException(exception, this)));
        }

        // Replace these demo callbacks with the game SDK/services.
        // Wallet balances remain the source of entitlement ownership.
        private void ApplyNoAdsDemo() => LogEntitlementEffect("No Ads enabled");

        private void ApplyLifetimeVipDemo() => LogEntitlementEffect("VIP enabled");

        private void UnlockStarterFundDemo() => LogEntitlementEffect("Starter Fund unlocked");

        private void UnlockSeason1PremiumPassDemo() => LogEntitlementEffect("Season 1 Premium Pass unlocked");

        private void LogEntitlementEffect(string effect)
        {
            Debug.Log($"[Shop] {effect}", this);
        }

        private void DisposeEntitlementEffects()
        {
            foreach (var binding in entitlementBindings) binding.Dispose();
            entitlementBindings.Clear();
        }
    }
}
