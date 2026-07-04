using Dreamy.DataConfig;

namespace Dreamy.Template
{
    [DataConfig("templateConfig")]
    public sealed class TemplateConfig : ConfigBase
    {
        public int StartingCoins { get; set; }
    }
}