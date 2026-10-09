using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    // Kept for existing prefab references. GameInstaller registers presenters in the shared factory.
    [RequireComponent(typeof(RateUsPanel))]
    public sealed class RateUsPanelLauncher : MonoBehaviour
    {
    }
}
