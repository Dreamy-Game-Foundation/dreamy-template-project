using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.UI
{
    /// <summary>
    /// Project-wide defaults loaded once from Resources/Dreamy/UI/TweenPresetLibrary.
    /// Individual players never need to assign a default preset.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TweenPresetLibrary",
        menuName = "Dreamy/UI/Tween Preset Library")]
    public sealed class TweenPresetLibrary : ScriptableObject
    {
        public const string ResourcePath = "Dreamy/UI/TweenPresetLibrary";

        [SerializeField] private List<TweenDefaultPreset> presets = new List<TweenDefaultPreset>();
        private static TweenPresetLibrary cached;

        public static TweenPresetLibrary Load()
        {
            if (cached == null)
            {
                cached = Resources.Load<TweenPresetLibrary>(ResourcePath);
            }

            return cached;
        }

        /// <summary>Library mapping first, then the original Resources/Tween preset path.</summary>
        public static TweenSettings Resolve(TweenEffectType type)
        {
            TweenSettings preset = Load()?.Get(type);
            return preset != null
                ? preset
                : Resources.Load<TweenSettings>("Tween/" + type + "TweenSettings");
        }

        public TweenSettings Get(TweenEffectType type)
        {
            foreach (TweenDefaultPreset entry in presets)
            {
                if (entry != null && entry.Type == type && entry.Preset != null)
                {
                    return entry.Preset;
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public sealed class TweenDefaultPreset
    {
        [SerializeField] private TweenEffectType type;
        [SerializeField] private TweenSettings preset;

        public TweenEffectType Type => type;
        public TweenSettings Preset => preset;
    }
}
