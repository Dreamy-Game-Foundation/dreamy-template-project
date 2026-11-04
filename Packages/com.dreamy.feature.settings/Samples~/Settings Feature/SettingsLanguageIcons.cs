using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    /// <summary>Game-owned presentation data; locale identity remains with the language gateway.</summary>
    [CreateAssetMenu(menuName = "Dreamy/Settings/Language Icons")]
    public sealed class SettingsLanguageIcons : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string LocaleCode;
            public Sprite Icon;
        }

        [SerializeField] private List<Entry> icons = new();

        public Sprite Find(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode)) return null;
            foreach (var entry in icons)
                if (entry != null && string.Equals(entry.LocaleCode, localeCode, StringComparison.OrdinalIgnoreCase))
                    return entry.Icon;
            return null;
        }
    }
}
