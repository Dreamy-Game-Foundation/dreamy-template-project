using Dreamy.Core;
using UnityEngine;

namespace Dreamy.Template.Demo
{
    public struct DemoScoreChangedEvent : IEvent
    {
        public int Score;
        public int Delta;
    }

    public sealed class DemoSession
    {
        public BindableProperty<int> Score { get; }
        public BindableProperty<float> Health { get; }

        public DemoSession(int score, float health)
        {
            Score = new BindableProperty<int>(score);
            Health = new BindableProperty<float>(health);
        }

        public void AddScore(int amount) => SetScore(Score.Value + amount);
        public void Damage(float amount) => SetHealth(Health.Value - amount);
        public void Heal(float amount) => SetHealth(Health.Value + amount);

        public void SetScore(int value)
        {
            int delta = value - Score.Value;
            Score.Value = value;
            if (delta == 0) return;

            MyEventBus<DemoScoreChangedEvent>.Raise(new DemoScoreChangedEvent
            {
                Score = Score.Value,
                Delta = delta
            });
        }

        private void SetHealth(float value)
        {
            Health.Value = Mathf.Clamp(value, 0f, 100f);
        }
    }
}
