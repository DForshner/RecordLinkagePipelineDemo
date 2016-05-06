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
    public class ProductPriceOutlierClassifer
    {
        private const decimal LOWER_RANGE_MULTIPLIER = 0.8M;
        private const decimal UPPER_RANGE_MULTIPLIER = 3M;
        private const int MIN_NUM_LISTINGS = 5;

        private IDictionary<string, ExchangeRate> _ratesBySource;

        public ProductPriceOutlierClassifer(IEnumerable<ExchangeRate> rates)
        {
            _ratesBySource = rates.ToDictionary(x => x.SourceCurrencyCode);
        }

        public IEnumerable<Tuple<Listing, bool>> ClassifyAsCamera(ProductMatch match)
        {
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
                .InterquartileWeakOutlierRange();

            // Recalculate the range again after throwing away the outliers
            var adjustedRange = withNormalizedPrices
                .Where(x => x.NormalizedPrice >= originalRange.Item1 && x.NormalizedPrice <= originalRange.Item2)
                .Select(x => x.NormalizedPrice)
                .ToList()
                .InterquartileWeakOutlierRange();

            // Some of the cameras that come are parts of kits are many times more
            // expensive than the camera alone so we need to raise the upper limit.
            var min = (adjustedRange.Item1 > 0 ? adjustedRange.Item1 : 0) * LOWER_RANGE_MULTIPLIER;
            var max = adjustedRange.Item2 * UPPER_RANGE_MULTIPLIER;

            var classifiedListings = withNormalizedPrices
                .Select(x => Tuple.Create(x.Listing, (x.NormalizedPrice >= min && x.NormalizedPrice <= max)));


            // TODO
            //if (classifiedListings.Any(x => x.Item1.Title.Contains("body") && x.Item2))
                //Debugger.Break();


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