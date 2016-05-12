using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Domain;
using Pipeline.Infrastructure;

namespace Pipeline.Matching
{
    /// <summary>
    /// Finds the best matching product for each listing.
    /// </summary>
    internal class ProductModelMatcher
    {
        public Tuple<IEnumerable<ProductMatch>, IEnumerable<Listing>> FindProductMatches(ManufacturerNameListingsBlock listingBlock, ManufacturerNameProductsBlock productBlock)
        {
            Debug.Assert(listingBlock.ManufacturerName == productBlock.ManufacturerName, "Expected same manufacturer name");

            var tokenCount = new Dictionary<string, int>();

            var products = new List<Tuple<Product, List<string>, List<string>>>();
            foreach(var product in productBlock.Products)
            {
                var modelTokens = product.Model.TokenizeOnWhiteSpace();
                var modelNGrams = modelTokens.CreateNShingles(1, Math.Max(modelTokens.Length, 3)).ToList();

                var familyTokens = !String.IsNullOrEmpty(product.Family) ? product.Family.TokenizeOnWhiteSpace() : new string[0];
                var familyNGrams = familyTokens.CreateNShingles(1, Math.Max(modelTokens.Length, 3)).ToList();

                products.Add(Tuple.Create(product, modelNGrams, familyNGrams));
            }

            var listings = new List<Tuple<Listing, List<string>>>();
            foreach(var listing in listingBlock.Listings)
            {
                var tokens = listing.Title.TokenizeOnWhiteSpace();
                var ngrams = tokens.CreateNShingles(1, Math.Min(tokens.Length, 3)).ToList();

                ngrams.ForEach(x => tokenCount.AddOrIncrement(x));

                listings.Add(Tuple.Create(listing, ngrams));
            }

            var totalTokens = listingBlock.Listings.Count;

            var productMatches = new Dictionary<Product, List<Listing>>();
            var unmatched = new List<Listing>();
            foreach (var listing in listings)
            {
                var listingTokens = listing.Item2;

                float bestScore = float.MinValue;
                Product bestMatch = null;
                foreach (var product in products)
                {
                    var modelTokens = product.Item2;

                    var modelMatches = listingTokens.Where(x => modelTokens.Contains(x)).ToList();
                    if (!modelMatches.Any())
                    {
                        continue;
                    }
                    var modelScore = modelMatches.Select(x => tokenCount[x]).CalculateEntropy(totalTokens);

                    // Only look for family tokens that are not already contained in the model
                    var familyTokens = product.Item3.Where(x => !modelTokens.Contains(x));
                    var familyMatches = listingTokens.Where(x => familyTokens.Contains(x)).ToList();
                    var familyScore = familyMatches.Select(x => tokenCount[x]).CalculateEntropy(totalTokens);

                    var score = modelScore + familyScore;
                    if (score < 0F + float.Epsilon || score < bestScore)
                    {
                        continue;
                    }

                    // We should be able to arrange the matching fragments in such a way that the product's
                    // model and family can be produced.  For now I'm just going to check that there are enough characters
                    // to fully produce the model and family.
                    if (!CheckFragmentsCouldMakeString(product.Item1.Model, modelMatches.Concat(familyMatches)))
                    {
                        continue;
                    }

                    // TODO: Excluding listings that don't contain the family may be too aggressive.
                    if (familyMatches.Any() && !CheckFragmentsCouldMakeString(product.Item1.Family, modelMatches.Concat(familyMatches)))
                    {
                        continue;
                    }

                    bestScore = score;
                    bestMatch = product.Item1;
                }

                if (bestMatch == null)
                {
                    // No good product match found for this listing
                    unmatched.Add(listing.Item1);
                }
                else
                {
                    if (!productMatches.ContainsKey(bestMatch)) { productMatches.Add(bestMatch, new List<Listing>()); }
                    productMatches[bestMatch].Add(listing.Item1);
                }
            }

            return Tuple.Create(productMatches.Select(x => new ProductMatch(x.Key, x.Value)), unmatched.AsEnumerable());
        }

        /// <summary>
        /// Check that if the fragments are arranged in an overlapping fashion they could produce the target string.
        /// </summary>
        private static bool CheckFragmentsCouldMakeString(string target, IEnumerable<string> fragments)
        {
            if (String.IsNullOrEmpty(target)) { return true; }

            var requiredChars = new string(target.Where(x => !Char.IsWhiteSpace(x)).ToArray());
            var isCharCovered = new BitArray(requiredChars.Length);

            // Slide each fragment over the string and when the fragment lines up mark the characters that are covered by the fragment.
            foreach(var fragment in fragments)
            {
                var startIdx = requiredChars.IndexOf(fragment);
                if (startIdx < 0) { continue; }

                // The fragment exists in the string so mark the characters as covered
                for(var i = startIdx; i < (startIdx + fragment.Length); ++i)
                {
                    isCharCovered[i] = true;
                }
            }

            // See if any character weren't covered by the fragments
            foreach(bool isCovered in isCharCovered)
            {
                if (!isCovered)
                    return false;
            }

            return true;
        }
    }
}