using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Settings;
using Dreamy.UI;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    [RequireComponent(typeof(SettingsPanel))]
    public sealed class SettingsPanelLauncher : MonoBehaviour
    {
        private const string RateUsPanelAddress = "Panel/RateUsPanel.prefab";
        private SettingsPanel panel;
        private SettingsPresenter presenter;
        private bool isOpeningRateUs;

        private void Awake()
        {
            panel = GetComponent<SettingsPanel>();
            presenter = new SettingsPresenter(ServiceLocator.Get<ISettingsService>(), panel);
            panel.OnPreShow += presenter.Show;
            panel.OnPostHide += presenter.Dispose;
            panel.OpenRateUsRequested += OpenRateUs;
        }

        private void OpenRateUs() => OpenRateUsAsync().Forget();

        private async UniTask OpenRateUsAsync()
        {
            if (isOpeningRateUs) return;
            isOpeningRateUs = true;
            try
            {
                await PanelManager.Instance.Transition<RateUsPanel>(RateUsPanelAddress);
            }
            finally
            {
                isOpeningRateUs = false;
            }
        }

        private void OnDestroy()
        {
            if (panel != null)
            {
                panel.OnPreShow -= presenter.Show;
                panel.OnPostHide -= presenter.Dispose;
                panel.OpenRateUsRequested -= OpenRateUs;
            }

            presenter?.Dispose();
        }
    }
}
