using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Fallback method of generating aliases
    /// </summary>
    public class DeterministicAliasGenerator
    {
        public IEnumerable<ManufacturerNameAlias> Generate()
        {
            return new[]
            {
                new ManufacturerNameAlias { Canonical = "fujifilm", Alias = "fuji" }
            };
        }
    }
}