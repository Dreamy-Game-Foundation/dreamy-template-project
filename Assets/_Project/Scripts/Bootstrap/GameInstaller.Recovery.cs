using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Economy;
using Dreamy.Economy.UI;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Shop;
using Dreamy.Shop.UI;
using Dreamy.UI;

namespace Dreamy.Template
{
    public sealed partial class GameInstaller
    {
        private IInsufficientResourcesPresentation shopResourceRecovery;

        private void InstallShopResourceRecovery(ShopCatalogConfig catalog)
        {
            var coin = new ResourceId("currency.coin");
            var gem = new ResourceId("currency.gem");
            var destinations = new ShopResourceDestinationMap(new Dictionary<ResourceId, string>
            {
                [coin] = "currency",
                [gem] = "currency"
            });
            var navigator = new ShopPanelResourceNavigator(async token =>
            {
                token.ThrowIfCancellationRequested();
                if (!PanelManager.HasInstance || !PanelManager.Instance.TryGet<ShopPanel>(out var panel))
                    return null;
                await panel.Show();
                token.ThrowIfCancellationRequested();
                return panel;
            });
            var acquisition = new ResourceAcquisitionCoordinator(new ShopResourceAcquisitionGateway(
                catalog, destinations, navigator, walletBalanceSource));
            var routes = new Dictionary<ResourceId, IInsufficientResourcesPresentation>
            {
                [coin] = ResourceSuggestionInstaller.Install<CoinSuggestionPanel>(presenterFactory,
                    coin, Address.CoinSuggestionPanel, walletBalanceSource, acquisition),
                [gem] = ResourceSuggestionInstaller.Install<GemSuggestionPanel>(presenterFactory,
                    gem, Address.GemSuggestionPanel, walletBalanceSource, acquisition)
            };
            shopResourceRecovery = new ResourceRecoveryPresentationRouter(walletBalanceSource, routes);
        }

        private async UniTask RecoverShopResourcesAsync(ShopPurchaseResult result, CancellationToken token)
        {
            if (result.Offer == null || result.Status != ShopPurchaseStatus.InsufficientResources) return;
            await shopResourceRecovery.ShowAsync(new[] { result.Offer.Cost }, "shop:" + result.Offer.Id, token);
        }
    }
}
