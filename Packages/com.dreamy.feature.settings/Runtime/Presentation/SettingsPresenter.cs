using System;
using System.Threading;
using Dreamy.UI;
using Cysharp.Threading.Tasks;

namespace Dreamy.Settings
{
    public sealed class SettingsPresenter : IPanelPresenter
    {
        private readonly ISettingsService service;
        private readonly ISettingsView view;
        private readonly Action openRateUs;
        private bool isBound;
        private int generation;
        private CancellationTokenSource languageLifetime;

        public SettingsPresenter(ISettingsService service, ISettingsView view, Action openRateUs = null)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.openRateUs = openRateUs;
        }

        public void Show()
        {
            Bind();
            view.SetPlatformActionsInteractable(true);
            Refresh();
        }

        public void Dispose()
        {
            if (!isBound)
            {
                return;
            }

            isBound = false;
            generation++;
            languageLifetime?.Cancel(); languageLifetime?.Dispose(); languageLifetime = null;
            if (view is ISettingsLanguageView languageView) languageView.LanguageRequested -= SetLanguage;
            if (service is ISettingsLanguageService languageService) languageService.LanguageChanged -= RefreshLanguage;
            view.MusicVolumeChanged -= SetMusicVolume;
            view.SfxVolumeChanged -= SetSfxVolume;
            view.HapticsEnabledChanged -= SetHapticsEnabled;
            view.GdprRequested -= ShowGdprConsent;
            view.RestorePurchasesRequested -= RestorePurchases;
            view.OpenRateUsRequested -= OpenRateUs;
            view.CloseRequested -= Close;
        }

        private void Bind()
        {
            if (isBound)
            {
                return;
            }

            generation++;
            languageLifetime = new CancellationTokenSource();
            if (view is ISettingsLanguageView languageView) languageView.LanguageRequested += SetLanguage;
            if (service is ISettingsLanguageService languageService) languageService.LanguageChanged += RefreshLanguage;
            view.MusicVolumeChanged += SetMusicVolume;
            view.SfxVolumeChanged += SetSfxVolume;
            view.HapticsEnabledChanged += SetHapticsEnabled;
            view.GdprRequested += ShowGdprConsent;
            view.RestorePurchasesRequested += RestorePurchases;
            view.OpenRateUsRequested += OpenRateUs;
            view.CloseRequested += Close;
            isBound = true;
        }

        private void OpenRateUs() => openRateUs?.Invoke();

        private void Refresh() { view.Render(service.GetState()); RefreshLanguage(); }
        private void RefreshLanguage()
        {
            if (!isBound || view is not ISettingsLanguageView languageView) return;
            languageView.RenderLanguage(service is ISettingsLanguageService languageService ? languageService.GetLanguageState() : default);
        }
        private void SetLanguage(string code)
        {
            if (service is ISettingsLanguageService languageService && isBound)
                languageService.SetLanguageAsync(code, languageLifetime.Token).Forget();
        }

        private void SetMusicVolume(float volume)
        {
            service.SetMusicVolume(volume);
            Refresh();
        }

        private void SetSfxVolume(float volume)
        {
            service.SetSfxVolume(volume);
            Refresh();
        }

        private void SetHapticsEnabled(bool enabled)
        {
            service.SetHapticsEnabled(enabled);
            Refresh();
        }

        private void ShowGdprConsent() => RunOperation(service.ShowGdprConsentAsync()).Forget();
        private void RestorePurchases() => RunOperation(service.RestorePurchasesAsync()).Forget();

        private async UniTaskVoid RunOperation(UniTask<SettingsOperationResult> operation)
        {
            int operationGeneration = generation;
            view.SetPlatformActionsInteractable(false);
            try
            {
                await operation;
                if (isBound && generation == operationGeneration) Refresh();
            }
            finally
            {
                if (isBound && generation == operationGeneration)
                    view.SetPlatformActionsInteractable(true);
            }
        }

        private void Close()
        {
            Dispose();
            view.Close();
        }
    }
}
