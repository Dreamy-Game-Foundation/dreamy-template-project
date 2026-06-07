using System;
using Dreamy.Template.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class TapTarget : PooledBehaviour
    {
        private Button button;
        private Action<TapTarget> onTapped;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        public void Configure(Action<TapTarget> callback)
        {
            onTapped = callback;
        }

        public override void OnDespawn()
        {
            onTapped = null;
        }

        private void HandleClick()
        {
            onTapped?.Invoke(this);
        }
    }
}
