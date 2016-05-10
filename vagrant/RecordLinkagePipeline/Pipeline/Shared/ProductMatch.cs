﻿using System;
using System.Collections.Generic;

namespace Pipeline.Shared
{
    /// <summary>
    /// A product and its matching listings
    /// </summary>
    public class ProductMatch
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