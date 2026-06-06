using Dreamy.DataConfig;

namespace Dreamy.Template
{
    [DataConfig("gameSettings")]
    public sealed class GameSettingsConfig : ConfigBase
    {
        public int StartingCoins { get; set; }

        public float MusicVolume { get; set; }
    }
}
