using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pipeline.Shared
{
    public class ProductMatchDto
    {
        [JsonProperty(PropertyName = "product_name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "listings")]
        public ICollection<ListingDto> Listings { get; set; }
    }
}
