using System;
using System.Collections.Generic;

namespace Pipeline.Domain
{
    /// <summary>
    /// A product and its matching listings
    /// </summary>
    internal class ProductMatch
    {
        public Product Product { get; private set; }
        public ICollection<Listing> Listings { get; private set; }

        public ProductMatch(Product product, ICollection<Listing> listings)
        {
            if (product == null) { throw new ArgumentNullException("product"); }
            if (listings == null) { throw new ArgumentNullException("listings"); }

            this.Product = product;
            this.Listings = listings;
        }
    }
}