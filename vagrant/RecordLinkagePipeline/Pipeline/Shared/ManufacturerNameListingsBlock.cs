using System.Collections.Generic;

namespace Pipeline.Shared
{
    public class ManufacturerNameListingsBlock
    {
        public string ManufacturerName { get; set; }
        public IEnumerable<Listing> Listings { get; set; }
    }
}