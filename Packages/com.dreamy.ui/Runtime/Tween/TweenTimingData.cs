using DG.Tweening;

namespace Dreamy.UI
{
    public readonly struct TweenTimingData
    {
        public TweenTimingData(
            Ease easeIn,
            Ease easeOut,
            float durationIn,
            float durationOut,
            float delayIn,
            float delayOut)
        {
            EaseIn = easeIn;
            EaseOut = easeOut;
            DurationIn = durationIn;
            DurationOut = durationOut;
            DelayIn = delayIn;
            DelayOut = delayOut;
        }

        public Ease EaseIn { get; }
        public Ease EaseOut { get; }
        public float DurationIn { get; }
        public float DurationOut { get; }
        public float DelayIn { get; }
        public float DelayOut { get; }

        public TweenTimingData WithDelays(float delayIn, float delayOut)
        {
            return new TweenTimingData(
                EaseIn,
                EaseOut,
                DurationIn,
                DurationOut,
                delayIn,
                delayOut);
        }
    }
}
