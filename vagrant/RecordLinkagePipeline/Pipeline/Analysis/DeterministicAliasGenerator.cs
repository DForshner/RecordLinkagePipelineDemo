using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    public class DeterministicAliasGenerator : IManufacturerNameAliasGenerator
    {
        public IEnumerable<ManufacturerNameAlias> Generate(ICollection<Product> products, ICollection<Listing> listings)
        {
            return new[]
            {
                new ManufacturerNameAlias { Canonical = "fujifilm", Alias = "fuji" }
            };
        }
    }
}
