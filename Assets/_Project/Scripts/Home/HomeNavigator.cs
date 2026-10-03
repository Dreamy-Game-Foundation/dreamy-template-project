using System;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Shop;
using Dreamy.UI;

namespace Dreamy.Template.Home
{
    public sealed class HomeNavigator : IDisposable
    {
        private bool isTransitioning;
        private ShopPanel shopPanel;
        private ShopPresenter shopPresenter;

        public async UniTask OpenAsync(HomeDestination destination)
        {
            if (isTransitioning)
            {
                return;
            }

            isTransitioning = true;
            try
            {
                switch (destination)
                {
                    case HomeDestination.Shop:
                        await OpenShopAsync();
                        return;
                    case HomeDestination.Settings:
                        UnityEngine.Debug.LogWarning("[Home] Settings is not installed yet.");
                        return;
                    default:
                        UnityEngine.Debug.LogWarning($"[Home] Unsupported destination: {destination}.");
                        return;
                }
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private async UniTask OpenShopAsync()
        {
            ReleaseShopPresenter();
            shopPanel = await PanelManager.Instance.Transition<ShopPanel>(Address.ShopPanel);
            shopPanel.Destroyed += ReleaseShopPresenter;
            shopPresenter = new ShopPresenter(ServiceLocator.Get<IShopService>(), shopPanel);
            shopPresenter.Show();
        }

        private void ReleaseShopPresenter()
        {
            if (shopPanel != null)
            {
                shopPanel.Destroyed -= ReleaseShopPresenter;
                shopPanel = null;
            }

            shopPresenter?.Dispose();
            shopPresenter = null;
        }

        public void Dispose() => ReleaseShopPresenter();
    }
}
