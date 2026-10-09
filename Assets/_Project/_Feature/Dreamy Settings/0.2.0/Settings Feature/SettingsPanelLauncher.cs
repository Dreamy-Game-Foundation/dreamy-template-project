using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    // Kept for existing prefab references. GameInstaller registers presenters in the shared factory.
    [RequireComponent(typeof(SettingsPanel))]
    public sealed class SettingsPanelLauncher : MonoBehaviour
    {
    }
}
