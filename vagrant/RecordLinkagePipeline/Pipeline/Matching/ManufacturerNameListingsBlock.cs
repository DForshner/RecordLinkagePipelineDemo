using System;
using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Matching
{
    /// <summary>
    /// Listings grouped by manufacturer name
    /// </summary>
    internal class ManufacturerNameListingsBlock
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