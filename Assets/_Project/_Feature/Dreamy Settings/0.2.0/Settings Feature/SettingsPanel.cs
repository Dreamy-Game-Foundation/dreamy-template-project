using System;
using Dreamy.Settings;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feature.Settings.Integration
{
    public sealed class SettingsPanel : UIPanel, ISettingsView
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Button gdprButton;
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Button openStoreButton;
        [SerializeField] private Button openRateUsButton;
        [SerializeField] private Button closeButton;

        public override bool CanBack => true;

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action GdprRequested;
        public event Action RestorePurchasesRequested;
        public event Action OpenStoreRequested;
        public event Action OpenRateUsRequested;
        public event Action CloseRequested;

        private void OnEnable()
        {
            musicToggle.onValueChanged.AddListener(RequestMusicToggle);
            sfxToggle.onValueChanged.AddListener(RequestSfxToggle);
            gdprButton.onClick.AddListener(RequestGdpr);
            restorePurchasesButton.onClick.AddListener(RequestRestorePurchases);
            openStoreButton.onClick.AddListener(RequestOpenStore);
            openRateUsButton.onClick.AddListener(RequestOpenRateUs);
            closeButton.onClick.AddListener(RequestClose);
        }

        protected override void OnDisable()
        {
            musicToggle.onValueChanged.RemoveListener(RequestMusicToggle);
            sfxToggle.onValueChanged.RemoveListener(RequestSfxToggle);
            gdprButton.onClick.RemoveListener(RequestGdpr);
            restorePurchasesButton.onClick.RemoveListener(RequestRestorePurchases);
            openStoreButton.onClick.RemoveListener(RequestOpenStore);
            openRateUsButton.onClick.RemoveListener(RequestOpenRateUs);
            closeButton.onClick.RemoveListener(RequestClose);
            base.OnDisable();
        }

        public void Render(SettingsViewState state)
        {
            musicToggle.SetIsOnWithoutNotify(state.MusicVolume > 0f);
            sfxToggle.SetIsOnWithoutNotify(state.SfxVolume > 0f);
            gdprButton.gameObject.SetActive(state.CanShowGdprConsent);
            restorePurchasesButton.gameObject.SetActive(state.CanRestorePurchases);
            openStoreButton.gameObject.SetActive(state.CanOpenStore);
        }

        public void SetPlatformActionsInteractable(bool interactable)
        {
            gdprButton.interactable = interactable;
            restorePurchasesButton.interactable = interactable;
            openStoreButton.interactable = interactable;
        }

        public void Close() => Hide();

        private void RequestMusicToggle(bool isOn) => MusicVolumeChanged?.Invoke(isOn ? 1f : 0f);
        private void RequestSfxToggle(bool isOn) => SfxVolumeChanged?.Invoke(isOn ? 1f : 0f);
        private void RequestGdpr() => GdprRequested?.Invoke();
        private void RequestRestorePurchases() => RestorePurchasesRequested?.Invoke();
        private void RequestOpenStore() => OpenStoreRequested?.Invoke();
        private void RequestOpenRateUs() => OpenRateUsRequested?.Invoke();
        private void RequestClose() => CloseRequested?.Invoke();
    }
}
