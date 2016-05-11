using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Analysis;
using Pipeline.Classification;
using Pipeline.Domain;
using Pipeline.Extraction;
using Pipeline.Infrastructure;
using Pipeline.Matching;
using Pipeline.Output;
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
            _log = log;
            _erates = erateLines.Select(ExchangeRateParser.Parse).ToList();

            // TODO: We could pass dependencies in via constructor injection if we wanted to unit test the pipeline.
            var config = ConfigParser.Parse(configLine);
            _productBlockGrouper = new ManufacturerProductsBlockGrouper();
            _aliases = new SimilarityAliasGenerator(config.ManufacturerNameCutoff, config.PossibleAliasPercentile, config.CommonWordPercentile);
            _productMatcher = new ProductModelMatcher();
            _naiveBayesClassifier = new NaiveBayesCameraClassifier(cameraTrainingSet.ToList(), accessoryTrainingSet.ToList(), config.MinNumWords, config.WordRatio);
            _heuristicClassifier = new HeuristicClassifier(_erates, config.LowPriceCutoff, config.HighPriceCutoff, config.Threshold);
            _priceClassifier = new ProductPriceOutlierClassifer(_erates, config.LowerRangeMultiplier, config.UpperRangeMultiplier);
        }

        public IEnumerable<ProductMatchDto> FindMatches(IEnumerable<string> productLines, IEnumerable<string> listingLines)
        {
            // TODO: Should be able to load files and parse as async tasks but ran into a bug when running mono on Ubuntu.  Something related to FileReader.Peek() blocking the thread and never resuming.
            var products = ParseOrLogException(productLines, ProductParser.Parse).ToList();
            var listings = ParseOrLogException(listingLines, ListingParser.Parse).ToList();

            var canonicalManufacturerNames = products.Select(x => x.Manufacturer).ToHashSet();

            var productBlocks = BlockProductsByManufacturer(products, canonicalManufacturerNames);

            var listingBlocks = BlockListingsByManufacturer(products, listings, canonicalManufacturerNames);

            var possibleMatches = MatchListingsToProduct(listingBlocks, productBlocks);

            var matches = PruneMatches(possibleMatches);

            return ProductMatchDtoMapper.Map(matches).ToList();
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

        private IEnumerable<ManufacturerNameListingsBlock> BlockListingsByManufacturer(ICollection<Product> products, ICollection<Listing> listings, HashSet<string> canonicalManufacturerNames)
        {
            _log("Blocking listings by manufacturer name");

            var probablityPerToken = TokenProbabilityCalculator.GetProbabilities(listings, x => x.Title);
            var aliases = _aliases.Generate(products, listings, probablityPerToken);
            var listingBlockGrouper = new ManufacturerListingsBlockGrouper(canonicalManufacturerNames, aliases);
            var listingBlocks = listingBlockGrouper.Match(listings);

            foreach (var unmatched in listingBlocks.Item2) { _log(String.Format("Failed to match listing manufacturer: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }

            return listingBlocks.Item1;
        }

        private IEnumerable<ProductMatch> MatchListingsToProduct(IEnumerable<ManufacturerNameListingsBlock> listingBlocks, IEnumerable<ManufacturerNameProductsBlock> productBlocks)
        {
            _log("Matching listings to products");

            var productBlocksByManufacturerName = productBlocks.ToDictionary(x => x.ManufacturerName);
            var toMatch = listingBlocks
                .Select(x => new { Listings = x, Products = productBlocksByManufacturerName.ContainsKey(x.ManufacturerName) ? productBlocksByManufacturerName[x.ManufacturerName] : null })
                .ToList();

            foreach(var pairs in toMatch.Where(x => x.Products == null))
            {
                LogUnmatchedListings(pairs.Listings.Listings);
            }

            var combinedMatches = new ConcurrentBag<ProductMatch>();
            toMatch.Where(x => x.Products != null)
                .AsParallel() // PERF: Matching pairs of products and listings in parallel gives this method a ~3x speedup
                .ForAll(x =>
                {
                    var matches = _productMatcher.FindProductMatches(x.Listings, x.Products);

                    LogUnmatchedListings(matches.Item2);

                    foreach (var match in matches.Item1) { combinedMatches.Add(match); }
                });
            return combinedMatches;
        }

        private void LogUnmatchedListings(IEnumerable<Listing> unmatched)
        {
            foreach (var listing in unmatched)
            {
                _log(String.Format("Failed to match listings to product: {0}, {1}", listing.Manufacturer, listing.Title));
            }
        }

        /// <summary>
        /// Remove non camera listings (accessories, batteries etc.) from product matches
        /// </summary>
        private IEnumerable<ProductMatch> PruneMatches(IEnumerable<ProductMatch> possibleMatches)
        {
            _log(String.Format("Pruning {0} matches", possibleMatches.Count()));

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

            foreach(var x in withClassification.Where(x => !x.IsCamera))
            {
                _log(String.Format("Pruned by Naive Bayes: [{0}, {1}, {2}] => {3}, {4} {5}", match.Product.Manufacturer, match.Product.Family, match.Product.Model, x.Listing.Title, x.Listing.Price, x.Listing.CurrencyCode));
            }

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

            foreach(var x in withClassification.Where(x => !x.IsCamera))
            {
                _log(String.Format("Pruned by heuristic: [{0}, {1}, {2}] => {3}, {4} {5}", match.Product.Manufacturer, match.Product.Family, match.Product.Model, x.Listing.Title, x.Listing.Price, x.Listing.CurrencyCode));
            }

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

            foreach(var x in withClassification.Where(x => !x.IsCamera))
            {
                _log(String.Format("Pruned by price outlier: [{0}, {1}, {2}] => {3}, {4} {5}", match.Product.Manufacturer, match.Product.Family, match.Product.Model, x.Listing.Title, x.Listing.Price, x.Listing.CurrencyCode));
            }

            var cameras = withClassification
                .Where(x => x.IsCamera)
                .Select(x => x.Listing)
                .ToList();
            return new ProductMatch(match.Product, cameras);
        }
    }
}
