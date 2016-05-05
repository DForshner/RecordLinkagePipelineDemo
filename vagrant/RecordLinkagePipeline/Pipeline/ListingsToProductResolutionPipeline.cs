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

        public ListingsToProductResolutionPipeline(Action<string> log)
        {
            _log = log;
        }

        public IEnumerable<ProductMatch> FindMatches(Func<IEnumerable<string>> loadRawProducts, Func<IEnumerable<string>> loadRawListings)
        {
            var products = Task.Run(() => ParseProducts(loadRawProducts()));
            var canonicalManufacturerNames = products
                .ContinueWith(x => new CanonicalManufacturerNameGenerator().Generate(x.Result));

            var listings = Task.Run(() => ParseListings(loadRawListings()));
            var probablityPerToken = listings
                .ContinueWith(x => TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(x.Result));

            var listingBlocks = Task.WhenAll(products, listings, canonicalManufacturerNames, probablityPerToken)
                .ContinueWith((x) => BlockListingsByManufacturer(products.Result, listings.Result, canonicalManufacturerNames.Result, probablityPerToken.Result));

            var productBlocks = Task.WhenAll(products, canonicalManufacturerNames)
                .ContinueWith((x) => BlockProductsByManufacturer(products.Result, canonicalManufacturerNames.Result));

            var possibleMatches = Task.WhenAll(listingBlocks, productBlocks, probablityPerToken)
                .ContinueWith((x) => MatchProductsToListings(listingBlocks.Result, productBlocks.Result, probablityPerToken.Result).ToList());

            var matches = PruneMatches(probablityPerToken.Result, possibleMatches.Result).ToList();

            return matches;
        }

        public IList<Product> ParseProducts(IEnumerable<string> rawStrings)
        {
            return rawStrings
                .AsParallel()
                .Select(x =>
                {
                    try
                    {
                        return ProductParser.Parse(x);
                    }
                    catch
                    {
                        _log(String.Format("Failed to parse: {0}", x));
                        return null;
                    }
                })
                .Where(x => x != null) // TODO: Something better
                .ToList();
        }

        public IList<Listing> ParseListings(IEnumerable<string> rawStrings)
        {
            return rawStrings
                .AsParallel()
                .Select(x =>
                {
                    try
                    {
                        return ListingParser.Parse(x);
                    }
                    catch
                    {
                        _log(String.Format("Failed to parse: {0}", x));
                        return null;
                    }
                })
                .Where(x => x != null) // TODO: Something better
                .ToList();
        }

        private IEnumerable<ProductMatch> PruneMatches(IDictionary<string, float> probablityPerToken, IEnumerable<ProductMatch> possibleMatches)
        {
            _log(String.Format("Pruning non camera listings"));

            var costClassifier = GetCostClassifier();
            var _accessoryClassifier = new DeterministicClassifier();

            foreach(var match in possibleMatches)
            {
                var accessoryScores = match.Listings
                    .Select(x => _accessoryClassifier.ClassifyAsCamera(probablityPerToken, x))
                    .Select(x => x ? 50 : 0)
                    .ToList();

                var costOutlierScores = costClassifier.ClassifyAsCamera(match)
                    .Select(x => x.Item2 ? 50 : 0)
                    .ToList();

                var listingScores = match.Listings
                    .Select((x, i) => new { Listing = x, Score = accessoryScores[i] + costOutlierScores[i] })
                    .ToList();

                listingScores
                    .Where(x => x.Score < 50)
                    .ToList()
                    .ForEach(x => _log(String.Format("Pruned listing: {0}", x.Listing.Title)));

                var cameraListings = listingScores
                    .Where(x => x.Score >= 50)
                    .Select(x => x.Listing)
                    .ToList();

                yield return new ProductMatch(match.Product, cameraListings);
            }
        }

        private static ProductPriceOutlierClassifer GetCostClassifier()
        {
            // TODO: Get rates from file
            // TODO: Add time ranges rate is valid for
            var rates = new[]
            {
                new ExchangeRate { SourceCurrencyCode = "cad", DestinationCurrencyCode = "cad", Rate = 1M },
                new ExchangeRate { SourceCurrencyCode = "usd", DestinationCurrencyCode = "cad", Rate = 1.29M },
                new ExchangeRate { SourceCurrencyCode = "eur", DestinationCurrencyCode = "cad", Rate = 1.40M },
                new ExchangeRate { SourceCurrencyCode = "gbp", DestinationCurrencyCode = "cad", Rate = 1.80M }
            };
            return new ProductPriceOutlierClassifer(rates);
        }

        private IEnumerable<ManufacturerNameProductsBlock> BlockProductsByManufacturer(ICollection<Product> products, HashSet<string> canonicalManufacturerNames)
        {
            var productBlockGrouper = new ManufacturerProductsBlockGrouper();
            return productBlockGrouper.Match(products, canonicalManufacturerNames);
        }

        private IEnumerable<ManufacturerNameListingsBlock> BlockListingsByManufacturer(ICollection<Product> products, ICollection<Listing> listings, HashSet<string> canonicalManufacturerNames, IDictionary<string, float> tokenProbablities)
        {
            _log(String.Format("Blocking listings by manufacturer name"));

            var aliases = new SimilarityAliasGenerator().Generate(products, listings, tokenProbablities);
            var listingBlockGrouper = new ManufacturerListingsBlockGrouper(canonicalManufacturerNames, aliases);
            var listingBlocks = listingBlockGrouper.Match(listings);

            foreach (var unmatched in listingBlocks.Item2) { _log(String.Format("Failed to match listing manufacturer: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }

            return listingBlocks.Item1;
        }

        private IEnumerable<ProductMatch> MatchProductsToListings(
            IEnumerable<ManufacturerNameListingsBlock> listingBlocks, IEnumerable<ManufacturerNameProductsBlock> productBlocks, IDictionary<string, float> probablityPerToken)
        {
            // TODO: Move to wire-up
            var matcher = new ProductModelMatcher();

            var productBlocksByManufacturerName = productBlocks.ToDictionary(x => x.ManufacturerName);
            foreach (var listingBlock in listingBlocks)
            {
                _log(String.Format("Matching listings to products for {0}", listingBlock.ManufacturerName));

                if (!productBlocksByManufacturerName.ContainsKey(listingBlock.ManufacturerName))
                {
                    foreach (var unmatched in listingBlock.Listings) { _log(String.Format("Failed to match listing to product: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }
                    continue; // No products for manufacturer
                }

                var productBlock = productBlocksByManufacturerName[listingBlock.ManufacturerName];
                var matches = matcher.FindProductMatchs(listingBlock, productBlock, probablityPerToken);

                foreach (var unmatched in matches.Item2) { _log(String.Format("Failed to match listing to product: {0}, {1}", unmatched.Manufacturer, unmatched.Title)); }

                foreach(var match in matches.Item1)
                {
                    yield return match;
                }
            }
        }
    }
}
