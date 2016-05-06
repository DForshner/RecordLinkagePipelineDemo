using System.Collections.Generic;
using System.Linq;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Classification
{
    /// <summary>
    /// Examine each listing and try to classify it as a camera using deterministic heuristics.
    ///
    /// Everything here was found by trial and error for the given dataset so this
    /// approach isn't going to scale gracefully as listings in new languages are added.
    /// </summary>
    internal class HeuristicClassifier
    {
        private static HashSet<string> _wordsAssociatedWithAccessoryListings = new[]
        {
            "bag",
            "body",
            "battery",
            "only",

            // Look for phrase "for {manufacturer name}" and "for {model}"
            "for",
            "für",
            "pour"
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

            // Look for phrase "camera with {feature}"
            "with",
            "livré avec"
        }.ToHashSet();

        public bool ClassifyAsCamera(Listing listing)
        {
            // Approach 1 - Examine listing price

            // TODO: Normalize price?
            // Score low prices (probability accessories) in proportion to how near zero they are.
            const int LOW_COST = 80;
            var lowCostScore = (listing.Price < LOW_COST)
                // Score is power of 2 to how near zero the price is
                ? -100 * (LOW_COST - (listing.Price * listing.Price) / LOW_COST) / LOW_COST
                : 0;

            // Approach 2 - Examine words in title.

            // Look for words typically associated with accessories
            var titleTokens = listing.Title.TokenizeOnWhiteSpace();
            const int NON_CAMERA_WORD_WEIGHT = -20;
            var accessoryWordsScore = _wordsAssociatedWithAccessoryListings
                .Where(x => titleTokens.Contains(x))
                .Select(x => NON_CAMERA_WORD_WEIGHT)
                .Sum();

            // Look for words typically associated with camera listings
            const int CAMERA_WORD_WEIGHT = 20;
            var cameraWordsScore = _wordsAssociatedWithCameraListings
                .Where(x => titleTokens.Contains(x))
                .Select(x => CAMERA_WORD_WEIGHT)
                .Sum();

            // Approach 3 - Examine sentence structure

            // Look for phrase "camera with {feature}"
            // TODO: Handle other languages. Ex: "Livré avec chargeur"
            var withWordIdx = titleTokens.LastIndexWhere(x => x == "with");
            var precededByCameraWord = (withWordIdx > 0 && titleTokens[withWordIdx - 1] == "camera");
            var phraseCameraWithFeatureScore = (withWordIdx > 0 && precededByCameraWord) ? 100 : 0;

            // Look for phrase "for {manufacturer name}" and "for {model}"
            // TODO: Handle other languages. Ex: "für coolpix", "pour lumix"
            // TODO: Check following word is a manufacturer name or product model
            var forWordIdx = titleTokens.LastIndexWhere(x => x == "for");
            var praseAccessoryForCameraScore = (forWordIdx > 0) ? -100 : 0;

            // Look for phrase "for {manufacturer/model}" with {feature}"
            // TODO: Check following word is a manufacturer name or product model
            var phraseAccessoryForCameraWithScore = (forWordIdx > 0 && withWordIdx > 0 && forWordIdx < withWordIdx) ? -100 : 0;

            // Combine scores together to produce a final score.
            var isCameraScore = 100
                + lowCostScore / 6
                + accessoryWordsScore / 6
                + cameraWordsScore / 6
                + phraseCameraWithFeatureScore / 6
                + praseAccessoryForCameraScore / 6
                + phraseAccessoryForCameraWithScore / 6;

            const int IS_CAMERA_THRESHOLD = 90;
            return isCameraScore > IS_CAMERA_THRESHOLD;
        }
    }
}