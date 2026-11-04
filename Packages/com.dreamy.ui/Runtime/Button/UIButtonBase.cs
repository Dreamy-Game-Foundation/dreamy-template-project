using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.UI
{
    public abstract class UIButtonBase : MonoBehaviour
    {
        [SerializeField] protected Button button;
        [SerializeField] private float clickCooldown = 0.2f;

        private float lastClickTime = -999f;

        protected virtual void Reset()
        {
            button = GetComponent<Button>();
        }

        protected virtual void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        protected virtual void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        protected abstract void OnClick();

        private void HandleClick()
        {
            if (Time.unscaledTime - lastClickTime < clickCooldown)
            {
                return;
            }

            lastClickTime = Time.unscaledTime;
            OnClick();
        }
    }
}
