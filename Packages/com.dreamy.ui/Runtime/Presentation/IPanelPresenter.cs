using System;

namespace Dreamy.UI
{
    /// <summary>Engine-independent presenter lifecycle for a panel opening.</summary>
    public interface IPanelPresenter : IDisposable
    {
        void Show();
    }
}
