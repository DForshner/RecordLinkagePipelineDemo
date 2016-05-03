using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Analysis;
using Pipeline.Extraction;
using Pipeline.Matching;
using Pipeline.Pruning;
using Pipeline.Shared;

namespace Pipeline
{
    /// <summary>
    /// Composition layer that coordinates the matching of listings to products
    /// </summary>
    public class SimplePipeline
    {
        private readonly Action<string> _logWarning;
        private readonly IManufacturerNameAliasGenerator _aliasGenerator;
        private readonly IListingPruner _accessoryPruner;

        public SimplePipeline(Action<string> logWarning, IManufacturerNameAliasGenerator aliasGenerator, IListingPruner accessoryPruner)
        {
            _aliasGenerator = aliasGenerator;
            _accessoryPruner = accessoryPruner;
            _logWarning = logWarning;
        }

        public IEnumerable<ProductMatch> FindMatches(ICollection<Product> products, ICollection<Listing> listings)
        {
            var mungedProducts = products.Select(Munge).ToList();

            var mungedListings = listings.Select(Munge).ToList();

            var probablityPerToken = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(mungedListings);

            var canonicalManufacturerNames = new CanonicalManufacturerNameGenerator().Generate(mungedProducts);

            var listingBlocks = BlockListingsByManufacturer(mungedProducts, mungedListings, canonicalManufacturerNames);

            var productBlocks = BlockProductsByManufacturer(mungedProducts, canonicalManufacturerNames);

            var possibleMatches = MatchProductsToListings(listingBlocks, productBlocks);

            var matches = Prune(probablityPerToken, possibleMatches).ToList();

            return matches;
        }

        /// <summary>
        /// TODO: Should probably be done while reading from file to reduce string object lifetime
        /// </summary>
        private static Listing Munge(Listing original)
        {
            return new Listing
            {
                Manufacturer = Munger.Munge(original.Manufacturer),
                Title = Munger.Munge(original.Title),
            };
        }

        /// <summary>
        /// TODO: Should probably be done while reading from file to reduce string object lifetime
        /// </summary>
        private static Product Munge(Product original)
        {
            return new Product
            {
                Manufacturer = Munger.Munge(original.Manufacturer),
                Model = Munger.Munge(original.Model),
            };
        }

        private IEnumerable<ProductMatch> Prune(IDictionary<string, float> probablityPerToken, IEnumerable<ProductMatch> possibleMatches)
        {
            // TODO: Get rates from file
            // TODO: Add time ranges rate is valid for
            var rates = new []
            {
                new ExchangeRate { SourceCurrencyCode = "USD", DestinationCurrencyCode = "CAD", Rate = 1.25M },
                new ExchangeRate { SourceCurrencyCode = "EUR", DestinationCurrencyCode = "CAD", Rate = 3.0M }
            };

            foreach(var match in possibleMatches)
            {
                var listings = new List<Listing>();


                foreach (var listing in match.Listings)
                {
                    var score = 0;
                    if (_accessoryPruner.ClassifyAsCamera(probablityPerToken, listing))
                    {
                        score += 50;
                    }

                    // TODO: Fix this API
                    if (new ProductMatchCostOutlierPruner(rates).Prune(new[] { match }).Any())
                    {
                        score += 50;
                    }

                    if (score <= 50)
                    {
                        yield return match;
                    }
                }

                yield return new ProductMatch(match.Product, listings);
            }
        }

        private IEnumerable<ManufacturerNameProductsBlock> BlockProductsByManufacturer(ICollection<Product> products, HashSet<string> canonicalManufacturerNames)
        {
            var productBlockGrouper = new ManufacturerProductsBlockGrouper();
            return productBlockGrouper.Match(products, canonicalManufacturerNames);
        }

        private IEnumerable<ManufacturerNameListingsBlock> BlockListingsByManufacturer(ICollection<Product> products, ICollection<Listing> listings, HashSet<string> canonicalManufacturerNames)
        {
            var aliases = _aliasGenerator.Generate(products, listings);
            var listingBlockGrouper = new ManufacturerListingsBlockGrouper(canonicalManufacturerNames, aliases);
            var listingBlocks = listingBlockGrouper.Match(listings);

            foreach (var unmatched in listingBlocks.Item2) { _logWarning(String.Format("Failed To match {0}", unmatched.Title)); }

            return listingBlocks.Item1;
        }

        private IEnumerable<ProductMatch> MatchProductsToListings(IEnumerable<ManufacturerNameListingsBlock> listingBlocks, IEnumerable<ManufacturerNameProductsBlock> productBlocks)
        {
            // TODO: Move to wire-up
            var matcher = new ProductModelMatcher();

            var productBlocksByManufacturerName = productBlocks.ToDictionary(x => x.ManufacturerName);
            foreach (var listingBlock in listingBlocks)
            {
                if (!productBlocksByManufacturerName.ContainsKey(listingBlock.ManufacturerName))
                {
                    foreach (var unmatched in listingBlock.Listings) { _logWarning(String.Format("Failed To match {0}", unmatched.Title)); }
                    continue; // No products for manufacturer
                }

                var productBlock = productBlocksByManufacturerName[listingBlock.ManufacturerName];
                var matches = matcher.FindProductMatchs(listingBlock, productBlock);

                foreach (var unmatched in matches.Item2) { _logWarning(String.Format("Failed To match {0}", unmatched.Title)); }

                foreach(var match in matches.Item1)
                {
                    yield return match;
                }
            }
        }
    }
}
