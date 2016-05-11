using System;
using System.Collections.Generic;
using Pipeline.Domain;
using Pipeline.Shared;

namespace Pipeline.Matching
{
    /// <summary>
    /// Products grouped by manufacturer name
    /// </summary>
    internal class ManufacturerNameProductsBlock
    {
        public string ManufacturerName { get; private set; }
        public ICollection<Product> Products { get; private set; }

        public ManufacturerNameProductsBlock(string manufacturerName, ICollection<Product> products)
        {
            if (String.IsNullOrEmpty(manufacturerName)) { throw new ArgumentNullException("manufacturerName"); }
            if (products == null) { throw new ArgumentNullException("products"); }

            this.ManufacturerName = manufacturerName;
            this.Products = products;
        }
    }
}