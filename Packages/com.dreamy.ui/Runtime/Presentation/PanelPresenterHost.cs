using System;

namespace Dreamy.UI
{
    /// <summary>Owns one presenter per opening; reusable when a cached view reopens.</summary>
    public sealed class PanelPresenterHost : IDisposable
    {
        private readonly PanelPresenterFactory factory;
        private readonly object view;
        private IPanelPresenter presenter;

        public PanelPresenterHost(PanelPresenterFactory factory, object view)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IPanelPresenter Presenter => presenter;

        public void Tick() => (presenter as ITickedPanelPresenter)?.Tick();

        public void Show()
        {
            if (presenter != null) return;
            presenter = factory.Create(view);
            if (presenter == null) return;
            try { presenter.Show(); }
            catch { Dispose(); throw; }
        }

        public void Dispose()
        {
            var current = presenter;
            presenter = null;
            current?.Dispose();
        }
    }
}
