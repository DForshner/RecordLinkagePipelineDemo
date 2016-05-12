using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Domain;
using Pipeline.Infrastructure;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Finds listings for similar manufacturer names and models to generate a list of aliases for manufacturers.
    /// Example: Many listings have a manufacturer of "fuji" but the products are listed under "fujifilm"
    ///
    /// For each product search for listings that have the product model in their title.  When a listing matches keep track the listing's manufacturer as
    /// a possible alias for the product's manufacturer.  The more instances that match the more confident we can be that the alias is true.
    ///
    /// TODO: This is going to get rather expensive on larger data sets.  We could probably get usable results doing this on a random sample basis.
    ///
    /// Prototype: https://github.com/DForshner/CSharpExperiments/blob/master/AliasGenerationBySimilarManufacturerAndModel.cs
    /// </summary>
    internal class SimilarityAliasGenerator
    {
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

        private readonly int _manufacturerNameCutoff;
        private readonly float _possibleAliasPercentile;
        private readonly float _commonWordPercentile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manufacturerNameCutoff">Note: needs to be low enough to match "fuji" up with "fujifilm"</param>
        /// <param name="possibleAliasPercentile">Words with scores above this percentile are considered good alias candidates.</param>
        /// <param name="commonWordPercentile">Words with occurrence probabilities above this percentile are considered common and won't be used when joining listings to products.</param>
        public SimilarityAliasGenerator(int manufacturerNameCutoff, float possibleAliasPercentile, float commonWordPercentile)
        {
            if (manufacturerNameCutoff < 0 || manufacturerNameCutoff > 100) { throw new ArgumentOutOfRangeException("manufacturerNameCutoff"); }
            if (possibleAliasPercentile < 0 || possibleAliasPercentile > 1) { throw new ArgumentOutOfRangeException("possibleAliasPercentile"); }
            if (commonWordPercentile < 0 || commonWordPercentile > 1) { throw new ArgumentOutOfRangeException("commonWordPercentile"); }

            _manufacturerNameCutoff = manufacturerNameCutoff;
            _possibleAliasPercentile = possibleAliasPercentile;
            _commonWordPercentile = commonWordPercentile;
        }

        public IEnumerable<ManufacturerNameAlias> Generate(ICollection<Product> products, ICollection<Listing> listings, IDictionary<string, float> tokenProbablities)
        {
            Debug.Assert(products != null, "expected products not null");
            Debug.Assert(products != null, "expected listings not null");
            Debug.Assert(tokenProbablities != null, "expected tokenProbablities not null");

            var commonTokens = GetCommonTokens(tokenProbablities);
            var modelShinglesByModelName = GenerateModelShingles(commonTokens, products);
            var manuNameNGramsByCanonicalManuName = GenerateManufacturerNameNGrams(products);

            // For each listing check all products for the best possible match
            var possibleAliasesByCanonical = new ConcurrentDictionary<PossibleAlias, float>();
            listings
                .AsParallel() // PERF: Process each listing in parallel
                .ForAll(listing =>
                {
                    var listingTitleTokens = new HashSet<string>(listing.Title.TokenizeOnWhiteSpace());

                    PossibleAlias bestMatch = null;
                    var bestScore = float.MaxValue;
                    foreach (var product in products)
                    {
                        // 1) Check that the listing and product manufacturer names are similar but not identical
                        // Ex: "fuji" is somewhat similar to "fujifilm"
                        var manuNameMatchScore = ScoreManufacturerNameSimilarity(product.Manufacturer, listing.Manufacturer, manuNameNGramsByCanonicalManuName);

                        const int PERFECT_MATCH = 100;
                        if (manuNameMatchScore == PERFECT_MATCH && product.Manufacturer == listing.Manufacturer)
                            continue; // Same manufacturer name so nothing to do

                        if (manuNameMatchScore < _manufacturerNameCutoff)
                            continue; // Names are too different

                        // 2) Check that the listing text contains the the product model
                        var modelMatchScore = ScoreListingTitleToProductModel(product.Model, listingTitleTokens, modelShinglesByModelName, tokenProbablities);

                        if (modelMatchScore < 0 + float.Epsilon)
                            continue; // No parts of the model name matched

                        // 3) Track of the best product model match for the current listing
                        if (modelMatchScore < bestScore)
                        {
                            bestScore = modelMatchScore;
                            bestMatch = new PossibleAlias { Canonical = product.Manufacturer, Alias = listing.Manufacturer };
                        }
                    }

                    if (bestMatch != null)
                    {
                        // Add or increase the scores for possible alias.
                        possibleAliasesByCanonical.AddOrUpdate(bestMatch, bestScore, (key, old) => old + bestScore);
                    }
                });

            // Return an upper percentile of aliases
            var percentileCutoff = (possibleAliasesByCanonical.Count > 1)
                ? possibleAliasesByCanonical.Values.ToList().FindPercentile(_possibleAliasPercentile)
                : 0F;
            return possibleAliasesByCanonical
                .Where(x => x.Value > percentileCutoff)
                .Select(x => new ManufacturerNameAlias(x.Key.Canonical, x.Key.Alias));
        }

        private HashSet<string> GetCommonTokens(IDictionary<string, float> tokenFrequencies)
        {
            var sortedFreqs = tokenFrequencies.Values.OrderBy(x => x).ToList();
            var percentialCutoff = sortedFreqs[(int)((float)sortedFreqs.Count * _commonWordPercentile)];
            return new HashSet<string>(tokenFrequencies.Where(x => x.Value > percentialCutoff).Select(x => x.Key));
        }

        private static IDictionary<string, List<string>> GenerateModelShingles(HashSet<string> commonTokens, ICollection<Product> products)
        {
            var modelShinglesByModelName = new Dictionary<string, List<string>>();
            foreach(var model in products.Select(x => x.Model))
            {
                if (modelShinglesByModelName.ContainsKey(model)) { continue; }

                var modelShingles = model
                    .TokenizeOnWhiteSpace()
                    .CreateNShingles(1, 2)
                    .Where(x => !commonTokens.Contains(x)) // Throw away products with a model that is too common.  Ex: Gets rid of "zoom" model.
                    .ToList();
                modelShinglesByModelName.Add(model, modelShingles);
            }
            return modelShinglesByModelName;
        }

        private static Dictionary<string, List<string>> GenerateManufacturerNameNGrams(ICollection<Product> products)
        {
            return products
                .Select(x => x.Manufacturer)
                .Distinct()
                .ToDictionary(x => x, x => x.CreateNGrams(2, 4).ToList());
        }

        private static float ScoreListingTitleToProductModel(string productModel, HashSet<string> listingTitleTokens, IDictionary<string, List<string>> modelShinglesByModelName, IDictionary<string, float> tokenProbablities)
        {
            var modelShingles = modelShinglesByModelName.ContainsKey(productModel)
                ? modelShinglesByModelName[productModel]
                : Enumerable.Empty<string>();

            return modelShingles
                .Where(x => listingTitleTokens.Contains(x))

                // Use the token's inverse probability for a score for now. The more unlikely the token is the less likely this is an accidental match.
                .Select(x => tokenProbablities.ContainsKey(x) ? (1 / tokenProbablities[x]) : 0F)
                .Sum();
        }

        /// <summary>
        /// Returns a score based on how many product manufacturer name n-grams match the listing manufacturer name n-grams
        /// </summary>
        private static int ScoreManufacturerNameSimilarity(string productManuName, string listingManuName, IDictionary<string, List<string>> manuNameNGramsByCanonicalManuName)
        {
            var manuNameNGrams = manuNameNGramsByCanonicalManuName.ContainsKey(productManuName)
                ? manuNameNGramsByCanonicalManuName[productManuName]
                : Enumerable.Empty<string>();

            var numMatchingNGrams = manuNameNGrams
                .Where(x => listingManuName.Contains(x))
                .Count();

            return (numMatchingNGrams > 0 && manuNameNGrams.Count() > 0)
                ? (100 * numMatchingNGrams) / manuNameNGrams.Count()
                : 0;
        }
    }
}