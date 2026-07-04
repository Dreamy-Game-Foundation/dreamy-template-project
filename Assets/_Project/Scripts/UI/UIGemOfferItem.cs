using TMPro;
using UnityEngine;

namespace Dreamy.Template.Demo
{
    public class UIGemOfferItem : UIResourceOfferItem
    {
        [SerializeField]
        private TextMeshProUGUI priceTxt;

        public override void Setup(OfferConfig offer)
        {
            base.Setup(offer);
            if (priceTxt != null)
            {
                priceTxt.text = offer.Price.ToCurrencyFormat();
            }
        }
    }
}
