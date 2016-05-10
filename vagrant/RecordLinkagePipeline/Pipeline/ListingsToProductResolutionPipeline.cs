using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pipeline.Analysis;
using Pipeline.Classification;
using Pipeline.Extraction;
using Pipeline.Matching;
using Pipeline.Shared;

namespace Pipeline
{
    /// <summary>
    /// Composition layer that coordinates the matching of listings to products
    /// </summary>
    public class ListingsToProductResolutionPipeline
    {
        private readonly Action<string> _log;
        private readonly ICollection<ExchangeRate> _erates;
        private readonly NaiveBayesCameraClassifier _naiveBayesClassifier;
        private readonly HeuristicClassifier _heuristicClassifier;
        private readonly ProductPriceOutlierClassifer _priceClassifier;
        private readonly SimilarityAliasGenerator _aliases;
        private readonly ProductModelMatcher _productMatcher;
        private readonly ManufacturerProductsBlockGrouper _productBlockGrouper;

        public ListingsToProductResolutionPipeline(Action<string> log, string configLine, IEnumerable<string> erateLines, IEnumerable<string> cameraTrainingSet, IEnumerable<string> accessoryTrainingSet)
        {
            log("Constructing pipeline");

            _log = log;
            _erates = erateLines.Select(ExchangeRateParser.Parse).ToList();

            // TODO: We could pass dependencies in via constructor injection if we wanted to unit test the pipeline.
            var config = ConfigParser.Parse(configLine);
            _productBlockGrouper = new ManufacturerProductsBlockGrouper();
            //_aliases = new SimilarityAliasGenerator(33, 0.50F, 0.90F);
            _aliases = new SimilarityAliasGenerator(config.ManufacturerNameCutoff, config.PossibleAliasPercentile, config.CommonWordPercentile);
            _productMatcher = new ProductModelMatcher();
            //_naiveBayesClassifier = new NaiveBayesCameraClassifier(cameraTrainingSet.ToList(), accessoryTrainingSet.ToList(), 3, 0.90F);
            _naiveBayesClassifier = new NaiveBayesCameraClassifier(cameraTrainingSet.ToList(), accessoryTrainingSet.ToList(), config.MinNumWords, config.WordRatio);
            //_heuristicClassifier = new HeuristicClassifier(_erates, 60M, 700M, 50F);
            _heuristicClassifier = new HeuristicClassifier(_erates, config.LowPriceCutoff, config.HighPriceCutoff, config.Threshold);
            //_priceClassifier = new ProductPriceOutlierClassifer(_erates, 0.5M, 5M);
            _priceClassifier = new ProductPriceOutlierClassifer(_erates, config.LowerRangeMultiplier, config.UpperRangeMultiplier);
        }

        public IEnumerable<ProductMatch> FindMatches(IEnumerable<string> productLines, IEnumerable<string> listingLines)
        {
            var products = Task.Run(() => ParseOrLogException(productLines, ProductParser.Parse).ToList());
            var listings = Task.Run(() => ParseOrLogException(listingLines, ListingParser.Parse).ToList());

            var canonicalManufacturerNames = products
                .ContinueWith(x => new CanonicalManufacturerNameGenerator().Generate(x.Result));

            var probablityPerToken = listings
                .ContinueWith(x => TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(x.Result));

            var listingBlocks = Task.WhenAll(products, listings, canonicalManufacturerNames, probablityPerToken)
                .ContinueWith((x) => BlockListingsByManufacturer(products.Result, listings.Result, canonicalManufacturerNames.Result, probablityPerToken.Result));

            var productBlocks = Task.WhenAll(products, canonicalManufacturerNames)
                .ContinueWith((x) => BlockProductsByManufacturer(products.Result, canonicalManufacturerNames.Result));

            var possibleMatches = Task.WhenAll(listingBlocks, productBlocks)
                .ContinueWith((x) => MatchProductsToListings(listingBlocks.Result, productBlocks.Result).ToList());

            var matches = PruneMatches(probablityPerToken.Result, possibleMatches.Result, _erates).ToList();

            return matches;
        }

        private IEnumerable<T> ParseOrLogException<T>(IEnumerable<string> lines, Func<string, T> parse) where T : class
        {
            foreach(var line in lines)
            {
                T result = null; // C# can't yield inside try with catch so store in temp.
                try
                {
                    result = parse(line);
                }
                catch (Exception ex)
                {
                    _log(String.Format("Failed to parse: {0} because {1}", line, ex.Message));
                }

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<ManufacturerNameProductsBlock> BlockProductsByManufacturer(ICollection<Product> products, HashSet<string> canonicalManufacturerNames)
        {
            _log("Blocking products by manufacturer name");

            return _productBlockGrouper.Match(products, canonicalManufacturerNames);
        }

        private IEnumerable<ManufacturerNameListingsBlock> BlockListingsByManufacturer(ICollection<Product> products, ICollection<Listing> listings, HashSet<string> canonicalManufacturerNames, IDictionary<string, float> tokenProbablities)
        {
            _log("Blocking listings by manufacturer name");

            var aliases = _aliases.Generate(products, listings, tokenProbablities);
            var listingBlockGrouper = new ManufacturerListingsBlockGrouper(canonicalManufacturerNames, aliases);
            var listingBlocks = listingBlockGrouper.Match(listings);

            foreach (var unmatched in listingBlocks.Item2) { _log(String.Format("Failed to match listing manufacturer: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }

            return listingBlocks.Item1;
        }

        private IEnumerable<ProductMatch> MatchProductsToListings(
            IEnumerable<ManufacturerNameListingsBlock> listingBlocks, IEnumerable<ManufacturerNameProductsBlock> productBlocks)
        {
            _log("Matching listings to products");

            var productBlocksByManufacturerName = productBlocks.ToDictionary(x => x.ManufacturerName);
            foreach (var listingBlock in listingBlocks)
            {
                _log(String.Format("Matching listings to products for {0}", listingBlock.ManufacturerName));

                if (!productBlocksByManufacturerName.ContainsKey(listingBlock.ManufacturerName))
                {
                    foreach (var unmatched in listingBlock.Listings) { _log(String.Format("Failed to match listings to product: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }
                    continue; // No products for this manufacturer
                }

                var productBlock = productBlocksByManufacturerName[listingBlock.ManufacturerName];
                var matches = _productMatcher.FindProductMatches(listingBlock, productBlock);

                foreach (var unmatched in matches.Item2) { _log(String.Format("Failed to match listings to product: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }

                foreach(var match in matches.Item1)
                {
                    yield return match;
                }
            }
        }

        /// <summary>
        /// Remove non camera listings (accessories, batteries etc.) from product matches
        /// </summary>
        private IEnumerable<ProductMatch> PruneMatches(IDictionary<string, float> probablityPerToken, IEnumerable<ProductMatch> possibleMatches, IEnumerable<ExchangeRate> erates)
        {
            _log("Pruning matches");

            foreach(var match in possibleMatches)
            {
                var afterBayesPruning = PruneByNaiveBayes(match);

                // I'm not terribly happy with this step.  Generating heuristics was a trial and error process that's probably going to break as soon as more non-English listings are added.
                var afterHeuristicPruning = PruneByHeuristic(afterBayesPruning);

                // The price outlier classifier has to be occur after the other classifiers.  Somewhat ironically the price outlier classifier is itself
                // susceptible to large numbers of outliers.  When there is too much variation in values the range ends up being so large that everything appears to be a typical value.
                var afterPriceOutlierPruning = PruneByPriceOutliers(afterHeuristicPruning);

                yield return afterPriceOutlierPruning;
            }
        }

        private ProductMatch PruneByNaiveBayes(ProductMatch match)
        {
            var withClassification =  match.Listings
                .Select(x => new { Listing = x, IsCamera = _naiveBayesClassifier.IsCamera(x) })
                .ToList();

            withClassification
                .Where(x => !x.IsCamera)
                .ToList()
                .ForEach(x => _log(String.Format("Pruned by Naive Bayes: [{0}, {1}, {2}] => {3}, {4} {5}", match.Product.Manufacturer, match.Product.Family, match.Product.Model, x.Listing.Title, x.Listing.Price, x.Listing.CurrencyCode)));

            var cameras = withClassification
                .Where(x => x.IsCamera)
                .Select(x => x.Listing)
                .ToList();
            return new ProductMatch(match.Product, cameras);
        }

        private ProductMatch PruneByHeuristic(ProductMatch match)
        {
            var withClassification =  match.Listings
                .Select(x => new { Listing = x, IsCamera = _heuristicClassifier.IsCamera(x) })
                .ToList();

            withClassification
                .Where(x => !x.IsCamera)
                .ToList()
                .ForEach(x => _log(String.Format("Pruned by heuristic: [{0}, {1}, {2}] => {3}, {4} {5}", match.Product.Manufacturer, match.Product.Family, match.Product.Model, x.Listing.Title, x.Listing.Price, x.Listing.CurrencyCode)));

            var cameras = withClassification
                .Where(x => x.IsCamera)
                .Select(x => x.Listing)
                .ToList();
            return new ProductMatch(match.Product, cameras);
        }

        private ProductMatch PruneByPriceOutliers(ProductMatch match)
        {
            var withClassification = _priceClassifier.ClassifyAsCamera(match)
                .Select(x => new { Listing = x.Item1, IsCamera = x.Item2 })
                .ToList();

            withClassification
                .Where(x => !x.IsCamera)
                .ToList()
                .ForEach(x => _log(String.Format("Pruned by price outlier: [{0}, {1}, {2}] => {3}, {4} {5}", match.Product.Manufacturer, match.Product.Family, match.Product.Model, x.Listing.Title, x.Listing.Price, x.Listing.CurrencyCode)));

            var cameras = withClassification
                .Where(x => x.IsCamera)
                .Select(x => x.Listing)
                .ToList();
            return new ProductMatch(match.Product, cameras);
        }
    }
}
