using System;
using System.Collections.Generic;

namespace Pipeline.Shared
{
    public class ManufacturerNameProductsBlock
    {
        public string ManufacturerName { get; private set; }
        public ICollection<Product> Products { get; private set; }

        public ManufacturerNameProductsBlock(string manufacturerName, ICollection<Product> products)
        {
            if (String.IsNullOrEmpty(manufacturerName)) { throw new ArgumentNullException("manufacturerName"); }
            if (products == null) { throw new ArgumentNullException("listings"); }

            this.ManufacturerName = manufacturerName;
            this.Products = products;
        }
    }
}