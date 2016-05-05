using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Fallback method of generating aliases
    /// </summary>
    public class DeterministicAliasGenerator
    {
        public IEnumerable<ManufacturerNameAlias> Generate(ICollection<Product> products, ICollection<Listing> listings, IDictionary<string, float> tokenProbablities)
        {
            return new[]
            {
                new ManufacturerNameAlias { Canonical = "fujifilm", Alias = "fuji" }
            };
        }
    }
}
