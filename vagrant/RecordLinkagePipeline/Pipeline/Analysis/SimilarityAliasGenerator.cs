using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Finds listings for similar manufacturer names and models to generate a list of aliases for manufacturers
    /// Prototype: https://github.com/DForshner/CSharpExperiments/blob/master/AliasGenerationBySimilarManufacturerAndModel.cs
    /// </summary>
    internal class SimilarityAliasGenerator
    {
        /// <summary>
        /// Note: needs to be low enough to match "fuji" up with "fujifilm"
        /// </summary>
        private const int MANUFACTURER_NAME_MATCH_CUTOFF = 33;

        /// <summary>
        /// Words with scores above this percentile are considered good alias candidates.
        /// </summary>
        private const float POSSIBLE_ALIAS_PERCENTILE = 0.50F;

        /// <summary>
        /// Words with occurrence probabilities above this percentile are considered common and won't be used when joining listings to products.
        /// </summary>
        private const float COMMON_WORD_PERCENTILE = 0.90F;

        private class PossibleAlias
        {
            public string Canonical { get; set; }
            public string Alias { get; set; }

            public override int GetHashCode()
            {
                var hash = 17;
                hash = 23 * hash + Canonical.GetHashCode();
                hash = 23 * hash + Alias.GetHashCode();
                return hash;
            }

            public override bool Equals(object obj)
            {
                var other = obj as PossibleAlias;
                if (other == null) { return false; }

                return this.Canonical == other.Canonical
                    && this.Alias == other.Alias;
            }
        }

        public IEnumerable<ManufacturerNameAlias> Generate(ICollection<Product> products, ICollection<Listing> listings, IDictionary<string, float> tokenProbablities)
        {
            var commonTokens = GetCommonTokens(tokenProbablities);
            var modelShinglesByModelName = Task.Run(() => GenerateModelNameShingles(commonTokens, products));
            var manuNameNGramsByCanonicalManuName = Task.Run(() => GenerateManufacturerNameNGrams(products));

            // For each listing check all product records for a the best possible match
            var possibleAliasesByCanonical = new ConcurrentDictionary<PossibleAlias, float>();
            listings
                .AsParallel()
                .ForAll(listing =>
                {
                    PossibleAlias bestMatch = null;
                    var bestModelMatchScore = float.MaxValue;

                    foreach (var product in products)
                    {
                        // 1) Check that the listing and product manufacturer names are similar
                        var manuNameMatchScore = CompareManufacturerNameSimilarity(manuNameNGramsByCanonicalManuName.Result, listing.Manufacturer, product.Manufacturer);

                        // Want "fuji" to be an alias for "fujifilm"
                        if (manuNameMatchScore < MANUFACTURER_NAME_MATCH_CUTOFF)
                            continue; // Names are too different

                        const int PERFECT_MATCH = 100;
                        if (manuNameMatchScore == PERFECT_MATCH && product.Manufacturer == listing.Manufacturer)
                            continue; // Same manufacturer name so nothing to do

                        // 2) Check that the listing text contains the the product model
                        var modelMatchScore = CompareListingTextToProductModel(tokenProbablities, modelShinglesByModelName.Result, listing.Title, product.Model);

                        if (modelMatchScore < 0 + float.Epsilon)
                            continue; // No parts of the model name matched

                        // Keep track of the best product model match for the current listing
                        if (modelMatchScore < bestModelMatchScore)
                        {
                            var canonical = product.Manufacturer;
                            var alias = listing.Manufacturer;

                            bestModelMatchScore = modelMatchScore;
                            bestMatch = new PossibleAlias { Canonical = canonical, Alias = alias };
                        }
                    }

                    // Add the possible alias scores together.
                    if (bestMatch != null)
                    {
                        possibleAliasesByCanonical.AddOrUpdate(bestMatch, bestModelMatchScore, (key, old) => old + bestModelMatchScore);
                    }
                });

            var percentileCutoff = (possibleAliasesByCanonical.Count > 1) ? possibleAliasesByCanonical.Values.ToList().FindPercentile(POSSIBLE_ALIAS_PERCENTILE) : 0F;

            return possibleAliasesByCanonical
                .Where(x => x.Value > percentileCutoff)
                .Select(x => new ManufacturerNameAlias { Canonical = x.Key.Canonical, Alias = x.Key.Alias });
        }

        private static float CompareListingTextToProductModel(IDictionary<string, float> tokenProbablities, IDictionary<string, List<string>> modelShinglesByModelName, string listingText, string productModel)
        {
            List<string> modelShingles;
            if (!modelShinglesByModelName.TryGetValue(productModel, out modelShingles))
            {
                return 0f;
            }

            var textTokens = new HashSet<string>(listingText.Split(null)); // null splits based on Unicode Char.IsWhiteSpace
            var modelScore = 0f;
            foreach (var nGram in modelShingles)
            {
                if (textTokens.Contains(nGram))
                {
                    // Use the token's inverse probability for a score for now
                    // The more unlikely the token is the less likely this is an accidental match.
                    modelScore += tokenProbablities.ContainsKey(nGram) ? 1 / tokenProbablities[nGram] : 0;
                }
            }

            return modelScore;
        }

        private static HashSet<string> GetCommonTokens(IDictionary<string, float> tokenFrequencies)
        {
            var sortedFreqs = tokenFrequencies.Values.OrderBy(x => x).ToList();
            var percentialCutoff = sortedFreqs[(int)((float)sortedFreqs.Count * COMMON_WORD_PERCENTILE)];
            return new HashSet<string>(tokenFrequencies.Where(x => x.Value > percentialCutoff).Select(x => x.Key));
        }

        private static int CompareManufacturerNameSimilarity(IDictionary<string, List<string>> manuNameNGramsByCanonicalManuName, string listingManuName, string productManuName)
        {
            List<string> manuNameNGrams;
            if (!manuNameNGramsByCanonicalManuName.TryGetValue(productManuName, out manuNameNGrams))
            {
                return 0;
            }

            var nameHit = 0;
            foreach (var nGram in manuNameNGrams)
            {
                if (listingManuName.Contains(nGram))
                {
                    nameHit++;
                }
            }

            return (100 * nameHit) / manuNameNGrams.Count;
        }

        private static IDictionary<string, List<string>> GenerateModelNameShingles(HashSet<string> commonTokens, ICollection<Product> products)
        {
            var modelShinglesByModelName = new ConcurrentDictionary<string, List<string>>();
            products
                .AsParallel()
                .ForAll(product =>
                {
                    var modelShingles = product.Model.CreateUniBiTokenShingles()
                        // Throw away products with a model that is too common.  Gets rid of "zoom" model.
                        .Where(x => !commonTokens.Contains(x))
                        .ToList();

                    if (modelShingles.Any())
                    {
                        modelShinglesByModelName.TryAdd(product.Model, modelShingles);
                    }
                });
            return modelShinglesByModelName;
        }

        private static IDictionary<string, List<string>> GenerateManufacturerNameNGrams(ICollection<Product> products)
        {
            var manuNameNGramsByCanonicalManuName = new ConcurrentDictionary<string, List<string>>();
            products
                .AsParallel()
                .ForAll(product => manuNameNGramsByCanonicalManuName.GetOrAdd(product.Manufacturer, x => x.CreateBiTriQuadCharacterNGrams().ToList()));
            return manuNameNGramsByCanonicalManuName;
        }
    }
}