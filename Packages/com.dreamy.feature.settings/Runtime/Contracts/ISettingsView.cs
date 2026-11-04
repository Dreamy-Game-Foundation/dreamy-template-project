using System;

namespace Dreamy.Settings
{
    public interface ISettingsView
    {
        event Action<float> MusicVolumeChanged;
        event Action<float> SfxVolumeChanged;
        event Action<bool> HapticsEnabledChanged;
        event Action GdprRequested;
        event Action RestorePurchasesRequested;
        event Action OpenRateUsRequested;
        event Action CloseRequested;

        void Render(SettingsViewState state);
        void SetPlatformActionsInteractable(bool interactable);
        void Close();
    }
}
