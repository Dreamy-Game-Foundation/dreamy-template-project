using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.UI
{
    [System.Serializable]
    public sealed class UIShineWavePattern
    {
        [SerializeField, Min(1)] private int minWaveCount = 1;
        [SerializeField, Min(1)] private int maxWaveCount = 3;
        [SerializeField] private Vector2 betweenWaveDelayRange = new Vector2(0.08f, 0.22f);
        [SerializeField] private Vector2 cooldownRange = new Vector2(0.7f, 1.8f);
        [SerializeField] private Vector2 durationRange = new Vector2(0.35f, 0.8f);
        [SerializeField] private Vector2 rotationRange = new Vector2(25f, 65f);

        internal int GetWaveCount()
        {
            return Random.Range(minWaveCount, maxWaveCount + 1);
        }

        internal float GetBetweenWaveDelay() => RandomRange(betweenWaveDelayRange);
        internal float GetCooldown() => RandomRange(cooldownRange);
        internal float GetDuration() => RandomRange(durationRange);
        internal float GetRotation() => RandomRange(rotationRange);

        internal void Validate()
        {
            minWaveCount = Mathf.Max(1, minWaveCount);
            maxWaveCount = Mathf.Max(minWaveCount, maxWaveCount);
            betweenWaveDelayRange = NormalizeRange(betweenWaveDelayRange, 0f);
            cooldownRange = NormalizeRange(cooldownRange, 0f);
            durationRange = NormalizeRange(durationRange, 0.01f);
            rotationRange = NormalizeRange(rotationRange, -180f);
        }

        private static float RandomRange(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }

        private static Vector2 NormalizeRange(Vector2 range, float minimum)
        {
            float min = Mathf.Max(minimum, range.x);
            return new Vector2(min, Mathf.Max(min, range.y));
        }
    }

    [DisallowMultipleComponent]
    public sealed class UIShineWaveController : MonoBehaviour, IShineWaveRegistry
    {
        [SerializeField] private bool findWavesInChildren = true;
        [SerializeField, Min(0.1f)] private float discoveryInterval = 0.5f;
        [SerializeField] private List<UIShineWave> shineWaves = new List<UIShineWave>();
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool startImmediately = true;
        [SerializeField] private List<UIShineWavePattern> wavePatterns =
            new List<UIShineWavePattern> { new UIShineWavePattern() };

        private readonly List<UIShineWave> availableWaves = new List<UIShineWave>();
        private readonly List<UIShineWavePattern> availablePatterns =
            new List<UIShineWavePattern>();
        private float nextWaveTime;
        private float nextDiscoveryTime;
        private int remainingWaves;
        private UIShineWavePattern activePattern;
        private bool isPlaying;

        private void Reset()
        {
            DiscoverWaves();
        }

        private void OnEnable()
        {
            if (findWavesInChildren)
            {
                DiscoverWaves();
            }

            foreach (UIShineWave shineWave in shineWaves)
            {
                if (shineWave == null) continue;
                shineWave.Stop();
                shineWave.SetPhase(1f);
            }

            isPlaying = playOnEnable;
            remainingWaves = 0;
            activePattern = null;
            nextWaveTime = startImmediately ? 0f : GetRandomPattern()?.GetCooldown() ?? 0f;
            nextDiscoveryTime = discoveryInterval;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnValidate()
        {
            discoveryInterval = Mathf.Max(0.1f, discoveryInterval);
            foreach (UIShineWavePattern pattern in wavePatterns)
            {
                pattern?.Validate();
            }
        }

        private void Update()
        {
            RefreshWavesIfNeeded();
            if (!isPlaying || shineWaves.Count == 0) return;

            nextWaveTime -= Time.unscaledDeltaTime;
            if (nextWaveTime > 0f) return;

            if (remainingWaves <= 0)
            {
                activePattern = GetRandomPattern();
                if (activePattern == null) return;

                remainingWaves = activePattern.GetWaveCount();
            }

            UIShineWave wave = GetRandomAvailableWave();
            if (wave == null)
            {
                nextWaveTime = 0.05f;
                return;
            }

            wave.PlayWave(activePattern.GetDuration(), activePattern.GetRotation());
            remainingWaves--;
            nextWaveTime = remainingWaves > 0
                ? activePattern.GetBetweenWaveDelay()
                : activePattern.GetCooldown();
        }

        public void Play()
        {
            isPlaying = true;
            nextWaveTime = 0f;
            remainingWaves = 0;
        }

        public void Stop()
        {
            isPlaying = false;
            remainingWaves = 0;
            activePattern = null;
            foreach (UIShineWave shineWave in shineWaves)
            {
                shineWave?.Stop();
            }
        }

        [ContextMenu("Find Shine Waves")]
        public void DiscoverWaves()
        {
            shineWaves.Clear();
            foreach (UIShineWave shineWave in GetComponentsInChildren<UIShineWave>(true))
            {
                Register(shineWave);
            }
        }

        public void Register(UIShineWave shineWave)
        {
            if (shineWave == null ||
                !IsNearestRegistry(shineWave) ||
                shineWaves.Contains(shineWave))
            {
                return;
            }

            shineWaves.Add(shineWave);
        }

        public void Unregister(UIShineWave shineWave)
        {
            shineWaves.Remove(shineWave);
        }

        private bool IsNearestRegistry(UIShineWave shineWave)
        {
            foreach (MonoBehaviour behaviour in shineWave.GetComponentsInParent<MonoBehaviour>(true))
            {
                if (behaviour is IShineWaveRegistry)
                {
                    return ReferenceEquals(behaviour, this);
                }
            }

            return false;
        }

        private UIShineWave GetRandomAvailableWave()
        {
            availableWaves.Clear();
            foreach (UIShineWave shineWave in shineWaves)
            {
                if (shineWave != null && shineWave.isActiveAndEnabled && !shineWave.IsPlaying)
                {
                    availableWaves.Add(shineWave);
                }
            }

            return availableWaves.Count == 0
                ? null
                : availableWaves[Random.Range(0, availableWaves.Count)];
        }

        private void RefreshWavesIfNeeded()
        {
            if (!findWavesInChildren) return;

            nextDiscoveryTime -= Time.unscaledDeltaTime;
            if (nextDiscoveryTime > 0f) return;

            DiscoverWaves();
            nextDiscoveryTime = discoveryInterval;
        }

        private UIShineWavePattern GetRandomPattern()
        {
            availablePatterns.Clear();
            foreach (UIShineWavePattern pattern in wavePatterns)
            {
                if (pattern != null)
                {
                    availablePatterns.Add(pattern);
                }
            }

            return availablePatterns.Count == 0
                ? null
                : availablePatterns[Random.Range(0, availablePatterns.Count)];
        }
    }
}
