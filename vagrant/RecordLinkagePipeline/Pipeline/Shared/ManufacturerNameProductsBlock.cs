using System.Collections.Generic;

namespace Pipeline.Shared
{
    public class ManufacturerNameProductsBlock
    {
        public string ManufacturerName { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}