using System;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Template.Demo;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Shop;
using Dreamy.UI;

namespace Dreamy.Template.Home
{
    public sealed class HomeNavigator : IDisposable
    {
        private bool isTransitioning;
        private bool disposed;
        private FoundationDemoRoot demo;
        private ShopPanel shopPanel;
        private ShopPresenter shopPresenter;

        public async UniTask OpenAsync(HomeDestination destination)
        {
            if (disposed || isTransitioning)
            {
                return;
            }

            isTransitioning = true;
            try
            {
                switch (destination)
                {
                    case HomeDestination.Demo:
                        await OpenDemoAsync();
                        return;
                    case HomeDestination.Shop:
                        await OpenShopAsync();
                        return;
                    case HomeDestination.Settings:
                        await PanelManager.Instance.Show<SettingsPanel>(Address.SettingsPanel);
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

        private async UniTask OpenDemoAsync()
        {
            FoundationDemoPanel panel = await PanelManager.Instance.Create<FoundationDemoPanel>(
                Address.FoundationDemoPanel);
            if (disposed) return;

            demo?.Dispose();
            demo = new FoundationDemoRoot(
                panel,
                ServiceLocator.Get<IDatasaveService>(),
                ServiceLocator.Get<IDataConfigService>());
            try
            {
                await PanelManager.Instance.Transition<FoundationDemoPanel>(Address.FoundationDemoPanel);
            }
            catch
            {
                demo.Dispose();
                demo = null;
                throw;
            }
        }

        private async UniTask OpenShopAsync()
        {
            ReleaseShopPresenter();
            ShopPanel panel = await PanelManager.Instance.Create<ShopPanel>(Address.ShopPanel);
            if (disposed) return;

            shopPanel = panel;
            try
            {
                shopPanel.OnPostHide += ReleaseShopPresenter;
                shopPresenter = new ShopPresenter(ServiceLocator.Get<IShopService>(), shopPanel);
                shopPresenter.Show();
                await PanelManager.Instance.Transition<ShopPanel>(Address.ShopPanel);
            }
            catch
            {
                ReleaseShopPresenter();
                throw;
            }
        }

        private void ReleaseShopPresenter()
        {
            if (shopPanel != null)
            {
                shopPanel.OnPostHide -= ReleaseShopPresenter;
                shopPanel = null;
            }

            shopPresenter?.Dispose();
            shopPresenter = null;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            demo?.Dispose();
            demo = null;
            ReleaseShopPresenter();
        }
    }
}
