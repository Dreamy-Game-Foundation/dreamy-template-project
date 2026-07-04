using System;
using System.Collections.Generic;
using Dreamy.DataConfig;
using Newtonsoft.Json;

namespace Dreamy.Template
{
    public enum EResourceOffer
    {
        Gem,
        Gold
    }

    [Serializable]
    public sealed class OfferConfig : DataConfigRow
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("offer_type")]
        public EResourceOffer OfferType { get; set; }

        [JsonProperty("icon_name")]
        public string IconName { get; set; }
    }

    [DataConfig("offerConfigs")]
    public sealed class OfferConfigTable : DataConfigTable<OfferConfig>
    {
        public List<OfferConfig> GetOffersByType(EResourceOffer offerType)
        {
            var results = new List<OfferConfig>();
            foreach (var offer in GetAll())
            {
                if (offer.OfferType == offerType)
                {
                    results.Add(offer);
                }
            }
            return results;
        }
    }
}
