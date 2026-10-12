using UnityEngine;

namespace Dreamy.Template.Feedback
{
    /// <summary>Basic mobile vibration with Editor simulation and host-controlled enablement.</summary>
    public static class MobileHaptics
    {
        // Platform support does not guarantee that a device has a vibration motor.
        public static bool IsSupported =>
#if UNITY_EDITOR
            true;
#else
            Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;
#endif

        public static bool IsEnabled { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => IsEnabled = false;

        public static void SetEnabled(bool enabled) => IsEnabled = enabled;

        /// <summary>Call on Unity's main thread. Duration is controlled by the OS.</summary>
        public static void Vibrate()
        {
            if (!IsSupported || !IsEnabled) return;
#if UNITY_EDITOR
            Debug.Log("[MobileHaptics] Simulated vibration.");
#else
            if (!Application.isFocused) return;
            Handheld.Vibrate();
#endif
        }
    }
}
