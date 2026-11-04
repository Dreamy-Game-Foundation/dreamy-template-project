using UnityEngine;

namespace Dreamy.UI
{
    [DisallowMultipleComponent]
    public sealed class UILayerRoot : MonoBehaviour
    {
        [SerializeField] private UILayer layer;

        public UILayer Layer => layer;

        public void SetLayer(UILayer value)
        {
            layer = value;
            RefreshName();
        }

        private void Reset()
        {
            RefreshName();
        }

        private void OnValidate()
        {
            RefreshName();
        }

        private void RefreshName()
        {
            gameObject.name = "Layer " + layer.ToString();
        }
    }
}