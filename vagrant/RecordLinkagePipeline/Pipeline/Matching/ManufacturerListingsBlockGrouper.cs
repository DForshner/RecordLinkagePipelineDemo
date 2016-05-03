using System.Collections.Generic;
using Pipeline.Shared;
using Pipeline.Infrastructure;
using System;
using System.Linq;

namespace Pipeline.Matching
{
    public class ManufacturerListingsBlockGrouper
    {
        private readonly HashSet<string> _canonical;
        private readonly IDictionary<string, string> _aliases;

        public ManufacturerListingsBlockGrouper(IEnumerable<string> canonicalManufacturerNames, IEnumerable<ManufacturerNameAlias> aliases)
        {
            _canonical = new HashSet<string>(canonicalManufacturerNames);
            _aliases = aliases.ToDictionary(x => x.Alias, x => x.Canonical);
        }

        public Tuple<IEnumerable<ManufacturerNameListingsBlock>, IEnumerable<Listing>> Match(IEnumerable<Listing> toMatch)
        {
            var matches = new Dictionary<string, List<Listing>>();
            var unmatched = new List<Listing>();

            foreach(var listing in toMatch)
            {
                var foundMatch = false;

                // 1) Try simple match on manufacturer name
                foreach(var token in listing.Manufacturer.TokenizeOnWhiteSpace())
                {
                    if (_canonical.Contains(token))
                    {
                        if (!matches.ContainsKey(token)) { matches.Add(token, new List<Listing>()); }
                        matches[token].Add(listing);

                        foundMatch = true;
                        break;
                    }
                }

                // 2) See if listing is using an alias
                if (!foundMatch)
                {
                    if (_aliases.ContainsKey(listing.Manufacturer))
                    {
                        var canonical = _aliases[listing.Manufacturer];
                        if (!matches.ContainsKey(canonical)) { matches.Add(canonical, new List<Listing>()); }
                        matches[canonical].Add(listing);

                        foundMatch = true;
                    }
                }

                // 3) Failed to match listing to manufacturer
                unmatched.Add(listing);
            }

            var blocks = matches.Select(x => new ManufacturerNameListingsBlock { ManufacturerName = x.Key, Listings = x.Value });

            return Tuple.Create(blocks, unmatched.AsEnumerable());
        }
    }
}