using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Dreamy.Assets;
using Cysharp.Threading.Tasks;

namespace Dreamy.Template.Demo
{
    public enum EResource
    {
        Gold,
        Gem
    }

    [System.Serializable]
    public struct ResourceValue
    {
        public EResource ResourceType;
        public int Value;

        public ResourceValue(EResource resourceType, int value)
        {
            ResourceType = resourceType;
            Value = value;
        }
    }

    public class UIResourceValue : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Image iconImage;

        public void SetValue(ResourceValue resourceValue)
        {
            if (valueText != null)
            {
                valueText.text = resourceValue.Value.ToString();
            }
        }
    }

    public static class CurrencyExtensions
    {
        public static string ToCurrencyFormat(this int priceInCents)
        {
            return (priceInCents / 100f).ToString("C2");
        }
    }

    public class UIResourceOfferItem : MonoBehaviour
    {
        [SerializeField] protected Image iconImg;
        [SerializeField] protected TextMeshProUGUI valueTxt;

        public OfferConfig Entity => offerEntity;
        protected OfferConfig offerEntity;

        public virtual void Setup(OfferConfig offer)
        {
            offerEntity = offer;

            // Load Sprite from IconMiscAtlas via AssetLoader
            AssetLoader.LoadSprite(Address.IconMiscAtlas, offer.IconName)
                .ContinueWith(sprite =>
                {
                    if (sprite != null && iconImg != null)
                    {
                        iconImg.sprite = sprite;
                        iconImg.SetNativeSize();
                    }
                });

            if (valueTxt != null)
            {
                valueTxt.text = offer.Value.ToString();
            }
        }
    }
}
