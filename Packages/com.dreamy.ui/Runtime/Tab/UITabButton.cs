using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.UI
{
    public class UITabButton : UIButtonBase
    {
        [SerializeField] protected bool useBackground = true;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Sprite activeBackgroundSprite;
        [SerializeField] protected Sprite inactiveBackgroundSprite;

        [SerializeField] protected bool useIcon;
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Sprite activeIconSprite;
        [SerializeField] protected Sprite inactiveIconSprite;

        [SerializeField] protected bool useText;
        [SerializeField] protected TextMeshProUGUI text;
        [SerializeField] protected Color activeTextColor = Color.white;
        [SerializeField] protected Color inactiveTextColor = Color.gray;

        private UITabControl control;
        private int id;

        protected override void Reset()
        {
            base.Reset();
            backgroundImage = GetComponent<Image>();
        }

        public void Register(UITabControl control, int id)
        {
            this.control = control;
            this.id = id;
        }

        public virtual void Init()
        {
        }

        public virtual void Active()
        {
            if (useBackground && backgroundImage != null)
            {
                backgroundImage.sprite = activeBackgroundSprite;
            }

            if (useIcon && iconImage != null)
            {
                iconImage.sprite = activeIconSprite;
            }

            if (useText && text != null)
            {
                text.color = activeTextColor;
            }
        }

        public virtual void Deactive()
        {
            if (useBackground && backgroundImage != null)
            {
                backgroundImage.sprite = inactiveBackgroundSprite;
            }

            if (useIcon && iconImage != null)
            {
                iconImage.sprite = inactiveIconSprite;
            }

            if (useText && text != null)
            {
                text.color = inactiveTextColor;
            }
        }

        protected override void OnClick()
        {
            control?.OpenTab(id);
        }
    }
}
