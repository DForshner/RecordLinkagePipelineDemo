using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Classification
{
    /// <summary>
    /// The same product model should have somewhat similar prices.
    /// Compares all listings for a particular model and remove any outliers.
    /// </summary>
    internal class ProductPriceOutlierClassifer
    {
        private const int MIN_NUM_LISTINGS = 5;

        private readonly IDictionary<string, ExchangeRate> _ratesBySource;
        private readonly decimal _lowerRangeMultiplier;
        private readonly decimal _upperRangeMultiplier;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rates">Exchange rates</param>
        /// <param name="lowerRangeMultipler">Multiple to increase the lower price range</param>
        /// <param name="upperRangeMultiplier">Multiple to increase the upper price range</param>
        public ProductPriceOutlierClassifer(IEnumerable<ExchangeRate> rates, decimal lowerRangeMultipler, decimal upperRangeMultiplier)
        {
            Debug.Assert(rates != null, "expected rates not null");
            if (lowerRangeMultipler < 0) { throw new ArgumentOutOfRangeException("lowPrice"); }
            if (upperRangeMultiplier < 0) { throw new ArgumentOutOfRangeException("highPrice"); }

            _ratesBySource = rates.ToDictionary(x => x.SourceCurrencyCode);
            _lowerRangeMultiplier = lowerRangeMultipler;
            _upperRangeMultiplier = upperRangeMultiplier;
        }

        public IEnumerable<Tuple<Listing, bool>> ClassifyAsCamera(ProductMatch match)
        {
            Debug.Assert(match != null, "expected match not null");

            if (match.Listings.Count < MIN_NUM_LISTINGS)
            {
                // Not enough points to determine what typical range of prices looks like so assume all are cameras
                return match.Listings.Select(x => Tuple.Create(x, true));
            }

            var withNormalizedPrices = match.Listings
                .Select(x => new { Listing = x, NormalizedPrice = GetPriceInCAD(x) })
                .ToList();

            var originalRange = withNormalizedPrices
                .Select(x => x.NormalizedPrice)
                .ToList()
                .InterquartileStrongOutlierRange();

            var min = (originalRange.Item1 > 0 ? originalRange.Item1 : 0) * _lowerRangeMultiplier;

            // Some of the listings are for cameras that come as kits are many times more
            // expensive than the camera alone so we need to raise the upper limit.
            var max = originalRange.Item2 * _upperRangeMultiplier;

            var classifiedListings = withNormalizedPrices
                .Select(x => Tuple.Create(x.Listing, (x.NormalizedPrice >= min && x.NormalizedPrice <= max)));

            return classifiedListings;
        }

        private decimal GetPriceInCAD(Listing listing)
        {
            if (listing.CurrencyCode == null || !_ratesBySource.ContainsKey(listing.CurrencyCode))
            {
                Debug.WriteLine("No exchange rate for source currency {0}", listing.CurrencyCode);
                return listing.Price;
            }

            return _ratesBySource[listing.CurrencyCode].Rate * listing.Price;
        }
    }
}