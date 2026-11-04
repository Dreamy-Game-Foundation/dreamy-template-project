using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.UI
{
    public class UIButtonClosePanel : UIButtonBase
    {
        [SerializeField] private UIPanel panel;

        protected override void Reset()
        {
            base.Reset();
            panel = GetComponentInParent<UIPanel>();
        }

        protected override void OnClick()
        {
            if (panel == null)
            {
                panel = GetComponentInParent<UIPanel>();
            }

            if (panel != null)
            {
                panel.Hide().Forget();
            }
        }
    }
}
