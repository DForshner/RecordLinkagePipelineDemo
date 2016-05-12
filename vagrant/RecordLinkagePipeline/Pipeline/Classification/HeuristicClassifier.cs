using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Domain;
using Pipeline.Infrastructure;

namespace Pipeline.Classification
{
    /// <summary>
    /// Classifies a listing as a camera using heuristics.
    ///
    /// Everything here was found by trial and error for the given dataset so I don't think
    /// approach is going to scale gracefully as listings in new languages are added.
    /// </summary>
    internal class HeuristicClassifier
    {
        private static HashSet<string> _wordsAssociatedWithAccessoryListings = new[]
        {
            "battery",
            "capacity",
        }.ToHashSet();

        private static HashSet<string> _wordsAssociatedWithCameraListings = new[]
        {
            "mp",
            "megapixel",
            "mega",
            "pixel",
            "mpix",
            "compact",
            "zoom",
            "optical",
            "stabilized",
            "digitalkamera",
            "digital",
            "camera",
        }.ToHashSet();

        private static HashSet<string> _forWords = new string[]
        {
            "for", // "for {model}"
            "für", // "für coolpix",
            "pour" //  "pour lumix"
        }.ToHashSet();

        private const int CAMERA_SCORE = 100;
        private const int NO_DECISION_SCORE = 50;
        private const int NON_CAMERA_SCORE = 0;

        private readonly IDictionary<string, ExchangeRate> _ratesBySource;
        private readonly decimal _lowPriceCutoff;
        private readonly decimal _highPriceCutoff;
        private readonly float _isCameraThreshold;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rates">Exchange rates</param>
        /// <param name="lowPrice">Treat listings with prices under this value as probably an accessory. Units: CAD $</param>
        /// <param name="highPrice">Treat listings with prices over this value as probably a camera. Units: CAD $</param>
        /// <param name="threshold">Score over which a listing should be classified as a camera. Range: [0-100]</param>
        public HeuristicClassifier(IEnumerable<ExchangeRate> rates, decimal lowPrice, decimal highPrice, float threshold)
        {
            Debug.Assert(rates != null, "expected rates not null");
            if (lowPrice < 0) { throw new ArgumentOutOfRangeException("lowPrice"); }
            if (highPrice < 0) { throw new ArgumentOutOfRangeException("highPrice"); }
            if (threshold < 0 || threshold > 100) { throw new ArgumentOutOfRangeException("threshold"); }

            _ratesBySource = rates.ToDictionary(x => x.SourceCurrencyCode);
            _lowPriceCutoff = lowPrice;
            _highPriceCutoff = highPrice;
            _isCameraThreshold = threshold;
        }

        public bool IsCamera(Listing listing)
        {
            Debug.Assert(listing != null, "expected listing not null");

            // Heuristic 1 - low listing prices are probably accessories and high are probably cameras
            var priceScore = ScorePriceListing(listing);

            // Heuristic 2 - Examine words in title.
            var titleTokens = listing.Title.TokenizeOnWhiteSpace();
            var accessoryWordsScore = ScoreWordsAssociatedWithNonCameraListings(titleTokens);
            var cameraWordsScore = ScoreWordsAssociatedWithCamera(titleTokens);

            // Heuristic 3 - Examine listing sentence structure
            var phraseCameraWithFeatureScore = ScorePhraseCameraWithFeature(titleTokens);
            var phraseAccessoryForCameraScore = ScorePhraseForManufacturerOrModel(titleTokens);

            // Combine individual [0-100] scores together into a final score.
            var isCameraScore =
                + priceScore * 0.20F
                + accessoryWordsScore * 0.25F
                + cameraWordsScore * 0.25F
                + phraseCameraWithFeatureScore * 0.15F
                + phraseAccessoryForCameraScore * 0.15F;

            Debug.Assert(isCameraScore >= 0 && isCameraScore <= 100, "Expected [0-100] range.");

            return isCameraScore >= _isCameraThreshold;
        }

        /// <summary>
        /// Score low prices (probability batteries, accessories, etc.) in proportion to how low the price is.
        ///
        /// 100 |                        +----------
        /// 75  |                        |
        /// 50  |        +{--------------|
        /// 25  |     /
        /// 5   | /
        /// 0   +--------LOW------------HIGH--------
        /// </summary>
        /// <returns>Score [0 - 100]</returns>
        private int ScorePriceListing(Listing listing)
        {
            var normalizedPrice = GetPriceInCAD(listing);

            if (normalizedPrice > _highPriceCutoff)
            {
                return CAMERA_SCORE; // camera
            }
            else if (normalizedPrice > _lowPriceCutoff)
            {
                return NO_DECISION_SCORE; // neutral
            }
            else
            {
                // accessory
                var ratio = ((normalizedPrice * normalizedPrice) / _lowPriceCutoff) / _lowPriceCutoff;
                return (int)(50M * ratio);
            }
        }

        // TODO: Duplicated in product price outlier classifier
        private decimal GetPriceInCAD(Listing listing)
        {
            if (listing.CurrencyCode == null || !_ratesBySource.ContainsKey(listing.CurrencyCode))
            {
                Debug.WriteLine("No exchange rate for source currency {0}", listing.CurrencyCode);
                return listing.Price;
            }

            return _ratesBySource[listing.CurrencyCode].Rate * listing.Price;
        }

        /// <summary>
        /// Look for words typically associated with accessories
        /// </summary>
        /// <returns>Score [0 - 100]</returns>
        private static int ScoreWordsAssociatedWithNonCameraListings(string[] titleTokens)
        {
            var accessoryWords = _wordsAssociatedWithAccessoryListings
                .Where(x => titleTokens.Contains(x))
                .Count();
            const int NON_CAMERA_WORD_WEIGHT = 20;
            var score = 50 - (NON_CAMERA_WORD_WEIGHT * accessoryWords) / _wordsAssociatedWithAccessoryListings.Count;
            return Math.Max(score, NON_CAMERA_SCORE);
        }

        /// <summary>
        /// Look for words typically associated with camera listings
        /// </summary>
        /// <returns>Score [0 - 100]</returns>
        private static int ScoreWordsAssociatedWithCamera(string[] titleTokens)
        {
            var cameraWords = _wordsAssociatedWithCameraListings
                .Where(x => titleTokens.Contains(x))
                .Count();
            const int CAMERA_WORD_WEIGHT = 20;
            var score = 50 + (CAMERA_WORD_WEIGHT * cameraWords);
            return Math.Min(score, CAMERA_SCORE);
        }

        /// <summary>
        /// Look for phrase "camera with {feature}"
        /// TODO: Handle other languages. Ex: "Livré avec chargeur"
        /// </summary>
        /// <returns>Score [0 - 100]</returns>
        private static int ScorePhraseCameraWithFeature(string[] titleTokens)
        {
            // phrase "camera with {feature}"
            var withWordIdx = titleTokens.LastIndexWhere(x => x == "with");
            var IsPrecededByCamera = (withWordIdx > 0 && titleTokens[withWordIdx - 1] == "camera");

            return (withWordIdx > 0 && IsPrecededByCamera) ? CAMERA_SCORE : NO_DECISION_SCORE;
        }

        /// <summary>
        /// Look for phrase "for {manufacturer name}" and "for {model}"
        /// TODO: Could check that the following word is a manufacturer name or product mode
        /// </summary>
        /// <returns>Score [0 - 100]</returns>>
        private static int ScorePhraseForManufacturerOrModel(string[] titleTokens)
        {
            return (_forWords.Any(x => titleTokens.Contains(x))) ? NON_CAMERA_SCORE : NO_DECISION_SCORE;
        }
    }
}