using System.Collections.Generic;
using Pipeline.Shared;
using System.Linq;

namespace Pipeline.Analysis
{
    internal class CanonicalManufacturerNameGenerator
    {
        public HashSet<string> Generate(IEnumerable<Product> products)
        {
            return new HashSet<string>(products.Select(x => x.Manufacturer));
        }
    }
}