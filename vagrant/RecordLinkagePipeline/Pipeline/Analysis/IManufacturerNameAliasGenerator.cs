using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    public interface IManufacturerNameAliasGenerator
    {
        IEnumerable<ManufacturerNameAlias> Generate(ICollection<Product> products, ICollection<Listing> listings, IDictionary<string, float> tokenProbablities);
    }
}
