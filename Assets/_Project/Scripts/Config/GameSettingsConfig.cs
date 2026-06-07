using Dreamy.DataConfig;

namespace Dreamy.Template
{
    [DataConfig("gameSettings")]
    public sealed class GameSettingsConfig : ConfigBase
    {
        public int StartingCoins { get; set; }

        public float MusicVolume { get; set; }

        public int TargetFrameRate { get; set; } = 60;

        public int PoolPreloadCount { get; set; } = 8;

        public float DemoDurationSeconds { get; set; } = 20f;
    }
}
