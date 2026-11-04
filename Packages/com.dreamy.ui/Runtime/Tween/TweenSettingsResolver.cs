using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    internal static class TweenSettingsResolver
    {
        private const float DefaultDurationIn = 0.25f;
        private const float DefaultDurationOut = 0.2f;
        private const Ease DefaultEaseIn = Ease.OutBack;
        private const Ease DefaultEaseOut = Ease.InBack;

        public static TweenTimingData Resolve(
            TweenSettings settings,
            bool overrideEaseIn,
            Ease easeIn,
            bool overrideEaseOut,
            Ease easeOut,
            bool overrideDurationIn,
            float durationIn,
            bool overrideDurationOut,
            float durationOut,
            float delayIn,
            float delayOut)
        {
            return new TweenTimingData(
                overrideEaseIn ? easeIn : settings ? settings.EaseIn : DefaultEaseIn,
                overrideEaseOut ? easeOut : settings ? settings.EaseOut : DefaultEaseOut,
                Mathf.Max(
                    0f,
                    overrideDurationIn
                        ? durationIn
                        : settings ? settings.DurationIn : DefaultDurationIn),
                Mathf.Max(
                    0f,
                    overrideDurationOut
                        ? durationOut
                        : settings ? settings.DurationOut : DefaultDurationOut),
                Mathf.Max(0f, delayIn),
                Mathf.Max(0f, delayOut));
        }
    }
}
