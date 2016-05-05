using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Matching
{
    /// <summary>
    /// Finds the best matching product for each listing.
    /// </summary>
    public class ProductModelMatcher
    {
        public Tuple<IEnumerable<ProductMatch>, IEnumerable<Listing>> FindProductMatchs(
            ManufacturerNameListingsBlock listingBlock, ManufacturerNameProductsBlock productBlock, IDictionary<string, float> probablityPerToken)
        {
            Debug.Assert(listingBlock.ManufacturerName == productBlock.ManufacturerName, "Expected same manufacturer name");

            var matches = new Dictionary<Product, List<Listing>>();
            var unmatched = new List<Listing>();

            foreach (var listing in listingBlock.Listings)
            {
                var listingUnigrams = listing.Title.TokenizeOnWhiteSpace();
                var bestScore = 0F;
                Product bestMatch = null;

                // TODO: Do n-gram matching on multi word model number so we don't throw
                // away token order information.

                // TODO: Normalize on model name length or longer model names get weighted more?

                foreach (var product in productBlock.Products)
                {
                    var productUnigrams = new HashSet<string>(product.Model.TokenizeOnWhiteSpace()); // TODO memoize
                    var score = 0F;

                    // 1) Try match on unigram tokens
                    foreach(var token in listingUnigrams)
                    {
                        if (!productUnigrams.Contains(token))
                        {
                            score = 0;
                            break;
                        }

                        score += probablityPerToken[token];
                    }

                    // 2) Try match on bigrams

                    if (score > bestScore)
                    {
                        bestScore = score / listingUnigrams.Length; // Normalize by # terms in model name so longer models aren't weighted extra
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