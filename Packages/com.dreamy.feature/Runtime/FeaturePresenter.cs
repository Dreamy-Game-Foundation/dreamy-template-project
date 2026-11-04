using System;

namespace Dreamy.Feature
{
    /// <summary>
    /// Reusable synchronous presenter lifecycle. Show binds once and refreshes on
    /// every call; Dispose unbinds once. A disposed presenter may be shown again.
    /// </summary>
    public abstract class FeaturePresenter : IFeaturePresenter, IDisposable
    {
        protected bool IsBound { get; private set; }

        public void Show()
        {
            try
            {
                if (!IsBound)
                {
                    // Set before Bind so cleanup also handles a partially failed bind.
                    IsBound = true;
                    Bind();
                }
                Refresh();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public abstract void Refresh();

        public void Dispose()
        {
            if (!IsBound) return;
            IsBound = false;
            Unbind();
        }

        // Hooks only manage owned subscriptions. Unbind must tolerate a partial
        // Bind and must not throw. All calls belong to the same owner thread.
        protected abstract void Bind();
        protected abstract void Unbind();
    }
}
