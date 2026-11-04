using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamy.UI
{
    [Serializable]
    public sealed class TweenTargetGroup
    {
        [SerializeField] private Transform target;
        [FormerlySerializedAs("effects")]
        [SerializeReference] private List<UITweenDefinition> tweens = new List<UITweenDefinition>();

        public Transform Target => target;
        public IReadOnlyList<UITweenDefinition> Tweens => tweens;

        internal void CollectTweens(List<ITween> destination, Component owner)
        {
            if (target == null) return;

            foreach (UITweenDefinition tween in tweens)
            {
                if (tween == null) continue;

                tween.Bind(target, TweenPresetLibrary.Resolve(tween.Type), owner);
                destination.Add(tween);
            }
        }
    }
}
