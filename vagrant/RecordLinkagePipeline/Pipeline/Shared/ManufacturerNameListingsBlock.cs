using System;
using System.Collections.Generic;

namespace Pipeline.Shared
{
    public class ManufacturerNameListingsBlock
    {
        public string ManufacturerName { get; private set; }
        public ICollection<Listing> Listings { get; private set; }

        public ManufacturerNameListingsBlock(string manufacturerName, ICollection<Listing> listings)
        {
            if (String.IsNullOrEmpty(manufacturerName)) { throw new ArgumentNullException("manufacturerName"); }
            if (listings == null) { throw new ArgumentNullException("listings"); }

            this.ManufacturerName = manufacturerName;
            this.Listings = listings;
        }
    }
}