using System;
using UnityEngine;

namespace Dreamy.UI
{
    [DisallowMultipleComponent]
    public sealed class PresenterViewHost : MonoBehaviour
    {
        private PanelPresenterHost host;
        private MonoBehaviour view;
        public void Initialize(PanelPresenterFactory factory, MonoBehaviour view)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (view == null) throw new ArgumentNullException(nameof(view));
            host?.Dispose();
            this.view = view;
            host = new PanelPresenterHost(factory, view);
            if (isActiveAndEnabled && view.isActiveAndEnabled) host.Show();
        }
        public void Release() { host?.Dispose(); host = null; view = null; }
        private void OnEnable() { if (view != null && view.isActiveAndEnabled) host?.Show(); }
        private void LateUpdate()
        {
            if (view != null && view.isActiveAndEnabled) { host?.Show(); host?.Tick(); }
            else host?.Dispose();
        }
        private void OnDisable() => host?.Dispose();
        private void OnDestroy() => Release();
    }
}
