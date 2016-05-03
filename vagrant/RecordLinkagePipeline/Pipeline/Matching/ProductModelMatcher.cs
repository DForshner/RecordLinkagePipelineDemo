using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Matching
{
    public class ProductModelMatcher
    {
        public Tuple<IEnumerable<ProductMatch>, IEnumerable<Listing>> FindProductMatchs(ManufacturerNameListingsBlock listingBlock, ManufacturerNameProductsBlock productBlock)
        {
            Debug.Assert(listingBlock.ManufacturerName == productBlock.ManufacturerName, "Expected same manufacturer name");

            var matches = new Dictionary<Product, List<Listing>>();
            var unmatched = new List<Listing>();

            foreach (var listing in listingBlock.Listings)
            {
                var tokens = listing.Title.TokenizeOnWhiteSpace();
                var bestScore = 0;
                Product bestMatch = null;

                foreach (var product in productBlock.Products)
                {
                    var modelTokens = new HashSet<string>(product.Model.TokenizeOnWhiteSpace()); // TODO memorize

                    var score = 0;
                    foreach(var token in tokens)
                    {
                        if (modelTokens.Contains(token))
                        {
                            score++;
                        }
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = product;
                    }
                }

                if (bestMatch == null)
                {
                    unmatched.Add(listing);
                    continue; // No product matches found for listing
                }

                if (!matches.ContainsKey(bestMatch)) { matches.Add(bestMatch, new List<Listing>()); }
                matches[bestMatch].Add(listing);
            }

            return Tuple.Create(matches.Select(x => new ProductMatch(x.Key, x.Value)), unmatched.AsEnumerable());
        }
    }
}