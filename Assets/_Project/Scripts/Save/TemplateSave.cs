using System;
using Dreamy.Datasave;
using Newtonsoft.Json;

namespace Dreamy.Template
{
    [Serializable]
    public sealed class TemplateSave : SaveData
    {
        public override int Version => 2;

        public int DemoOpenCount;
        public int CurrentScore;
        public int BestScore;
        public bool IsInitialized;

        // Write-only JSON aliases accept v1 data but are never emitted in v2 saves.
        [JsonProperty("LaunchCount")] private int LegacyLaunchCount { set => DemoOpenCount = value; }
        [JsonProperty("Coins")] private int LegacyCoins { set => CurrentScore = value; }
        [JsonProperty("Score")] private int LegacyScore { set => BestScore = value; }

        public override void Migrate(int fromVersion)
        {
            if (fromVersion < 2)
            {
                // Preserve an existing zero score instead of reseeding it from config.
                IsInitialized = true;
            }
        }
    }
}
