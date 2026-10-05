using Dreamy.Core;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    [RequireComponent(typeof(RateUsPanel))]
    public sealed class RateUsPanelLauncher : MonoBehaviour
    {
        private RateUsPanel panel;
        private RateUsPresenter presenter;

        private void Awake()
        {
            panel = GetComponent<RateUsPanel>();
            presenter = new RateUsPresenter(ServiceLocator.Get<ISettingsService>(), panel);
            panel.OnPreShow += presenter.Show;
            panel.OnPostHide += presenter.Dispose;
        }

        private void OnDestroy()
        {
            if (panel != null)
            {
                panel.OnPreShow -= presenter.Show;
                panel.OnPostHide -= presenter.Dispose;
            }

            presenter?.Dispose();
        }
    }
}
