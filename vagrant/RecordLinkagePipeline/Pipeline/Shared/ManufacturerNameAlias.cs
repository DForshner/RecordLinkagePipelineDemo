using System;

namespace Pipeline.Shared
{
    /// <summary>
    /// Manufacturer name alias
    /// </summary>
    public class ManufacturerNameAlias
    {
        public readonly string Canonical;
        public readonly string Alias;

        public ManufacturerNameAlias(string canonical, string alias)
        {
            if (String.IsNullOrEmpty(canonical)) { throw new ArgumentNullException("manufacturerName"); }
            if (String.IsNullOrEmpty(alias)) { throw new ArgumentNullException("alias"); }

            this.Canonical = canonical;
            this.Alias = alias;
        }
    }
}
