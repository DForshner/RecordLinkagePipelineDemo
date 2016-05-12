using System;
using Newtonsoft.Json;

namespace Pipeline.Shared
{
    public class ListingDto
    {
        [JsonProperty(PropertyName = "manufacturer")]
        public string Manufacturer { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "price")]
        public string Price { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }
}