using System;
using Dreamy.Datasave;
using UnityEngine.Serialization;

namespace Dreamy.Template
{
    [Serializable]
    public sealed class TemplateSave : SaveData
    {
        public int LaunchCount;
        public int Coins;
        public int Score;
    }
}