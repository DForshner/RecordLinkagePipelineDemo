using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    internal class PossibleAlias
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

    /// <summary>
    /// Create ngram/shingles
    /// </summary>
    public static class NGramBuilder
    {
        /// <summary>
        /// Generates character n-grams
        /// </summary>
        private static IEnumerable<string> CreateBiTriQuadCharacterNGrams(string str)
        {
            for (var i = 0; i < str.Length - 1; i++)
            {
                yield return new string(new char[] { str[i], str[i + 1] });
            }

            for (var i = 0; i < str.Length - 2; i++)
            {
                yield return new string(new char[] { str[i], str[i + 1], str[i + 2] });
            }

            for (var i = 0; i < str.Length - 3; i++)
            {
                yield return new string(new char[] { str[i], str[i + 1], str[i + 2], str[i + 3] });
            }
        }

        /// <summary>
        /// Generates word/token shingles
        /// </summary>
        private static IEnumerable<string> CreateUniBiTokenShingles(string str)
        {
            var tokens = str.Split(null); // null splits based on Unicode Char.IsWhiteSpace

            for (var i = 0; i < tokens.Length - 1; i++)
            {
                yield return tokens[i];
                yield return tokens[i] + tokens[i + 1];
            }

            if (tokens.Length > 0)
            {
                yield return tokens[tokens.Length - 1];
            }
        }

        public static IDictionary<string, List<string>> GenerateModelNameShingles(HashSet<string> commonTokens, ICollection<Product> products)
        {
            var modelShinglesByModelName = new ConcurrentDictionary<string, List<string>>();
            products
                .AsParallel()
                .ForAll(product =>
                {
                    var modelShingles = CreateUniBiTokenShingles(product.Model)
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

        public static IDictionary<string, List<string>> GenerateManufacturerNameNGrams(ICollection<Product> products)
        {
            var manuNameNGramsByCanonicalManuName = new ConcurrentDictionary<string, List<string>>();
            products
                .AsParallel()
                .ForAll(product => manuNameNGramsByCanonicalManuName.GetOrAdd(product.Manufacturer, x => CreateBiTriQuadCharacterNGrams(x).ToList()));
            return manuNameNGramsByCanonicalManuName;
        }
    }

    internal static class TokenStatistics
    {
        /// <summary>
        /// Words with occurrence probabilities above this percentile are considered common and shouldn't be used for joins.
        /// </summary>
        const float COMMON_WORD_PERCENTILE = 0.90F;

        public static Dictionary<string, float> GenerateTokenProbabilitiesPerListing(ICollection<Listing> listings)
        {
            var freqByToken = new ConcurrentDictionary<string, int>();
            var docCount = 0;
            listings
                .AsParallel()
                .ForAll(mungedListing =>
                {
                    foreach (var token in mungedListing.Title.Split(null))
                    {
                        freqByToken.AddOrUpdate(token, 1, (key, val) => { return val + 1; });
                    }

                    Interlocked.Increment(ref docCount);
                });

            // Turn freq into probability per doc
            return freqByToken.ToDictionary(x => x.Key, x => (float)x.Value / docCount);
        }

        public static HashSet<string> GetCommonTokens(IDictionary<string, float> tokenFrequencies)
        {
            var sortedFreqs = tokenFrequencies.Values.OrderBy(x => x).ToList();
            var percentialCutoff = sortedFreqs[(int)((float)sortedFreqs.Count * COMMON_WORD_PERCENTILE)];
            return new HashSet<string>(tokenFrequencies.Where(x => x.Value > percentialCutoff).Select(x => x.Key));
        }
    }

    /// <summary>
    /// Finds listings for similar manufacturer names and models to generate a list of aliases for manufacturers
    /// Earlier version: https://github.com/DForshner/CSharpExperiments/blob/master/AliasGenerationBySimilarManufacturerAndModel.cs
    /// </summary>
    public class SimilarityAliasGenerator : IManufacturerNameAliasGenerator
    {
        /// <summary>
        /// Words with scores above this percentile are considered good alias candidates.
        /// </summary>
        const float POSSIBLE_ALIAS_PERCENTILE = 0.50F;

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

        /// <summary>
        /// </summary>
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

        public IEnumerable<ManufacturerNameAlias> Generate(ICollection<Product> products, ICollection<Listing> listings)
        {
            var tokenProbablities = Task.Run(() => TokenStatistics.GenerateTokenProbabilitiesPerListing(listings));
            var commonTokens = tokenProbablities
                .ContinueWith(x => TokenStatistics.GetCommonTokens(x.Result));

            var manuNameNGramsByCanonicalManuName = Task.Run(() => NGramBuilder.GenerateManufacturerNameNGrams(products));

            var modelShinglesByModelName = commonTokens
                .ContinueWith(x => NGramBuilder.GenerateModelNameShingles(commonTokens.Result, products));

            // For each listing check all product records for a the best possible match
            var possibleAliasesByCanonical = new ConcurrentDictionary<PossibleAlias, float>();
            listings
                .AsParallel()
                .ForAll(listing =>
                {
                    PossibleAlias bestMatch = null;
                    float bestModelMatchScore = float.MaxValue;

                    foreach (var product in products)
                    {
                        // 1) Check that the listing and product manufacturer names are similar
                        var manuNameMatchScore = CompareManufacturerNameSimilarity(manuNameNGramsByCanonicalManuName.Result, listing.Manufacturer, product.Manufacturer);

                        // Want "fuji" to be an alias for "fujifilm"
                        const int PERCENT_MATCH_CUTOFF = 33;
                        if (manuNameMatchScore < PERCENT_MATCH_CUTOFF)
                            continue; // Names are too different
                        const int PERFECT_MATCH = 100;
                        if (manuNameMatchScore == PERFECT_MATCH && product.Manufacturer == listing.Manufacturer)
                            continue; // Same manufacturer name

                        // 2) Check that the listing text contains the the product model
                        var modelMatchScore = CompareListingTextToProductModel(tokenProbablities.Result, modelShinglesByModelName.Result, listing.Title, product.Model);

                        //if (listing.Manufacturer == "fuji" && modelMatchScore > 0.00000001f)
                            //Debugger.Break();

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

            var sortedScores = possibleAliasesByCanonical.Select(x => x.Value).OrderBy(x => x).ToList();
            var percentileCutoff = (sortedScores.Count > 1) ? sortedScores[(int)((float)(sortedScores.Count) * POSSIBLE_ALIAS_PERCENTILE)] : 0;
            return possibleAliasesByCanonical
                .Where(x => x.Value > percentileCutoff)
                .Select(x => new ManufacturerNameAlias { Canonical = x.Key.Canonical, Alias = x.Key.Alias });
        }
    }
}
