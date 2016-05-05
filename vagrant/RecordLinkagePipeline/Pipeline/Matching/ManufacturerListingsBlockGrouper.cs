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

                // 1) Common Case: Try a simple O(1) match on manufacturer name
                var listingTokens = listing.Manufacturer.TokenizeOnWhiteSpace();
                foreach(var token in listingTokens)
                {
                    if (_canonical.Contains(token))
                    {
                        AddMatch(matches, listing, token);
                        foundMatch = true;
                        break;
                    }
                }

                if (foundMatch) { continue; }

                // 2) Try matching tokens for names with multiple words (Ex: konica minolta)
                foreach (var canonical in _canonical)
                {
                    var canonicalTokens = canonical.TokenizeOnWhiteSpace().ToList(); // TODO: memoize
                    if (canonicalTokens.Count > 1 && canonicalTokens.Intersect(listingTokens).Any())
                    {
                        AddMatch(matches, listing, canonical);
                        foundMatch = true;
                        break;
                    }
                }

                if (foundMatch) { continue; }

                // 3) See if listing is using an alias
                if (_aliases.ContainsKey(listing.Manufacturer))
                {
                    var canonical = _aliases[listing.Manufacturer];
                    AddMatch(matches, listing, canonical);
                    foundMatch = true;
                }

                if (!foundMatch)
                {
                    // Failed to match listing to manufacturer
                    unmatched.Add(listing);
                }
            }

            var blocks = matches.Select(x => new ManufacturerNameListingsBlock { ManufacturerName = x.Key, Listings = x.Value });

            return Tuple.Create(blocks, unmatched.AsEnumerable());
        }

        private static void AddMatch(Dictionary<string, List<Listing>> matchesToUpdate, Listing listing, string canonicalName)
        {
            if (!matchesToUpdate.ContainsKey(canonicalName)) { matchesToUpdate.Add(canonicalName, new List<Listing>()); }
            matchesToUpdate[canonicalName].Add(listing);
        }
    }
}