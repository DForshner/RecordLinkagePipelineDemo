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
    internal class ProductModelMatcher
    {
        public Tuple<IEnumerable<ProductMatch>, IEnumerable<Listing>> FindProductMatchs(
            ManufacturerNameListingsBlock listingBlock, ManufacturerNameProductsBlock productBlock, IDictionary<string, float> probablityPerToken)
        {
            Debug.Assert(listingBlock.ManufacturerName == productBlock.ManufacturerName, "Expected same manufacturer name");

            var matches = new Dictionary<Product, List<Listing>>();
            var unmatched = new List<Listing>();

            foreach (var listing in listingBlock.Listings)
            {
                var listingShingles = listing.Title.CreateUniBiTokenShingles().ToArray();

                // Find the best product match
                var bestScore = 0F;
                Product bestMatch = null;
                foreach (var product in productBlock.Products)
                {
                    // TODO: memoize or pre-compute this product token stuff so it's not be recalculated each loop
                    var modelTokens = product.Model.TokenizeOnWhiteSpace();

                    var score = 0F;

                    // Handle case where model doesn't have product family name included
                    // TODO: Make this less ridiculous
                    var familyTokens = product.Family.TokenizeOnWhiteSpace();
                    var modelMissingFamily = familyTokens.Where(x => !modelTokens.Contains(x));
                    foreach(var token in modelMissingFamily)
                    {
                        if (!listing.Title.Contains(token))
                            score = float.MinValue;
                    }

                    // Check that every token exists in listing
                    foreach(var token in modelTokens)
                    {
                        if (!listing.Title.Contains(token))
                            score = float.MinValue;
                    }

                    var productTokensToCheck = (modelTokens.Length == 1)
                        ? modelTokens
                        : modelTokens.CreateBiTriTokenShingles().ToArray();

                    // TODO: Score using probability of term occurring in listings
                    foreach(var token in listingShingles)
                    {
                        if (productTokensToCheck.Contains(token))
                        {
                            score += 1;
                        }
                    }

                    if (score > bestScore)
                    {
                        bestScore = score / productTokensToCheck.Length; // Normalize by # terms in model name so longer model names aren't weighted extra
                        bestMatch = product;
                    }
                }

                if (bestMatch == null)
                {
                    // No product matches found for listing
                    unmatched.Add(listing);
                    continue;
                }

                if (!matches.ContainsKey(bestMatch)) { matches.Add(bestMatch, new List<Listing>()); }
                matches[bestMatch].Add(listing);
            }

            return Tuple.Create(matches.Select(x => new ProductMatch(x.Key, x.Value)), unmatched.AsEnumerable());
        }
    }
}