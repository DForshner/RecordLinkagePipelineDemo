using System.Collections.Generic;
using System.Linq;
using Pipeline.Domain;
using Pipeline.Shared;

namespace Pipeline.Matching
{
    /// <summary>
    /// Groups (blocks) products by manufacturer name.
    /// </summary>
    internal class ManufacturerProductsBlockGrouper
    {
        public IEnumerable<ManufacturerNameProductsBlock> Match(IEnumerable<Product> productsToMatch, IEnumerable<string> canonicalManufacturerNames)
        {
            var matched = new Dictionary<string, List<Product>>();

            foreach(var toMatch in productsToMatch)
            {
                if (!matched.ContainsKey(toMatch.Manufacturer))
                {
                    matched.Add(toMatch.Manufacturer, new List<Product>());
                }
                matched[toMatch.Manufacturer].Add(toMatch);
            }

            return matched.Select(x => new ManufacturerNameProductsBlock(x.Key, x.Value));
        }
    }
}