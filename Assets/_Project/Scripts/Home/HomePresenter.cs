using System;
using Cysharp.Threading.Tasks;

namespace Dreamy.Template.Home
{
    public sealed class HomePresenter : IDisposable
    {
        private readonly IHomeView view;
        private readonly HomeNavigator navigator;
        private bool disposed;

        public HomePresenter(IHomeView view, HomeNavigator navigator)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
            view.DestinationRequested += OnDestinationRequested;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            view.DestinationRequested -= OnDestinationRequested;
            navigator.Dispose();
        }

        private void OnDestinationRequested(HomeDestination destination)
        {
            OpenAsync(destination).Forget();
        }

        private async UniTaskVoid OpenAsync(HomeDestination destination)
        {
            view.SetNavigationInteractable(false);
            try
            {
                await navigator.OpenAsync(destination);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }
            finally
            {
                if (!disposed) view.SetNavigationInteractable(true);
            }
        }
    }
}
