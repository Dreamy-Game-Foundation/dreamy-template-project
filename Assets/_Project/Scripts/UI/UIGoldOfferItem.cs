using UnityEngine;

namespace Dreamy.Template.Demo
{
    public class UIGoldOfferItem : UIResourceOfferItem
    {
        [SerializeField]
        private UIResourceValue price;

        public override void Setup(OfferConfig offer)
        {
            base.Setup(offer);
            if (price != null)
            {
                price.SetValue(new ResourceValue(EResource.Gem, offer.Price));
            }
        }
    }
}
