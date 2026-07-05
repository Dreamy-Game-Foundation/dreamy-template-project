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

            // Load Sprite from ShopOfferAtlas via AssetLoader
            AssetLoader.LoadSprite(Address.ShopOfferAtlas, offer.IconName)
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
