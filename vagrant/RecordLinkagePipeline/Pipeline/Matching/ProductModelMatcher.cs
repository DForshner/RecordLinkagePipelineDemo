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
                var listingShingles = listing.Title.CreateUniBiTokenShingles().ToList();

                var bestScore = 0F;
                Product bestMatch = null;
                foreach (var product in productBlock.Products)
                {
                    var productTokens = product.Model.TokenizeOnWhiteSpace();
                    var productTokensToCheck = (productTokens.Length == 1) ? productTokens : productTokens.CreateBiTriTokenShingles();
                    var score = 0F;

                    // TODO: Score using probability of term occurring in listings

                    if (productTokens.Length == 1)
                    {
                        foreach(var token in listingShingles)
                        {
                            if (productTokens.First() == token)
                            {
                                score += 1;
                            }
                        }
                    }
                    else
                    {
                        var productShingles = productTokens.CreateBiTriTokenShingles();
                        foreach(var token in listingShingles)
                        {
                            if (productShingles.Contains(token))
                            {
                                score += 1;
                            }
                        }
                    }

                    if (score > bestScore)
                    {
                        bestScore = score / productTokens.Length; // Normalize by # terms in model name so longer model names aren't weighted extra
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