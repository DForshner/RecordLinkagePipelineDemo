using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Shared;
using Pipeline.Infrastructure;
using System;

namespace Pipeline.Pruning
{
    /// <summary>
    /// The same product model should have somewhat similar prices.
    /// Compares all listings for a particular model and remove any outliers.
    /// </summary>
    public class ProductMatchCostOutlierPruner
    {
        private const decimal OUTLIER_DEVIATIONS = 1.25M;
        private IDictionary<string, ExchangeRate> _ratesBySource;

        public ProductMatchCostOutlierPruner(IDictionary<string, ExchangeRate> ratesBySource)
        {
            _ratesBySource = ratesBySource;
        }

        public IEnumerable<ProductMatch> Prune(IEnumerable<ProductMatch> matches)
        {
            foreach(var match in matches)
            {
                var typicalListings = PruneListingsForProduct(match);
                yield return new ProductMatch(match.Product, typicalListings.ToList());
            }
        }

        private IEnumerable<Listing> PruneListingsForProduct(ProductMatch match)
        {
            var withNormalizedPrices = match.Listings
                .Select(x => new { Listing = x, NormalizedPrice = GetPriceInCAD(x) })
                .ToList();

            var range = withNormalizedPrices
                .Select(x => x.NormalizedPrice)
                .ToList()
                .InterquartileWeakOutlierRange();

            return withNormalizedPrices
                .Where(x => x.NormalizedPrice >= range.Item1 && x.NormalizedPrice <= range.Item2)
                .Select(x => x.Listing);
        }

        private decimal GetPriceInCAD(Listing listing)
        {
            if (!_ratesBySource.ContainsKey(listing.CurrencyCode))
            {
                Debug.WriteLine("No exchange rate for source currency {0}", listing.CurrencyCode);
            }

            return (_ratesBySource.ContainsKey(listing.CurrencyCode))
                ? _ratesBySource[listing.CurrencyCode].Rate * listing.Price
                : listing.Price;
        }
    }
}