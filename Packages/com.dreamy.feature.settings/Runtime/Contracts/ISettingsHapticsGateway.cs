namespace Dreamy.Settings
{
    /// <summary>Host-owned haptic preference, persistence, and application to the haptic provider.</summary>
    public interface ISettingsHapticsGateway
    {
        bool IsSupported { get; }
        bool IsEnabled { get; }
        void SetEnabled(bool enabled);
    }
}
