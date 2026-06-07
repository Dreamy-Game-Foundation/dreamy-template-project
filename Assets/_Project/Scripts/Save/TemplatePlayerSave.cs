using System;
using Dreamy.Datasave;

namespace Dreamy.Template
{
    [Serializable]
    public sealed class TemplatePlayerSave : SaveData
    {
        public int LaunchCount;
        public int Coins;
        public int TapRushHighScore;
    }
}
