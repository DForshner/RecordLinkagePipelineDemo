using System;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Manufacturer name alias
    /// </summary>
    internal class ManufacturerNameAlias
    {
        public readonly string Canonical;
        public readonly string Alias;

        public ManufacturerNameAlias(string canonical, string alias)
        {
            if (String.IsNullOrEmpty(canonical)) { throw new ArgumentNullException("canonical"); }
            if (String.IsNullOrEmpty(alias)) { throw new ArgumentNullException("alias"); }

            this.Canonical = canonical;
            this.Alias = alias;
        }
    }
}
