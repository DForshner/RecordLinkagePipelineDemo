using System.Collections.Generic;
using System.Linq;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Classification
{
    /// <summary>
    /// Examine each listing and try to classify it as a camera using deterministic rules/heuristics
    /// </summary>
    public class DeterministicClassifier
    {
        private static string[] _wordsAssociatedWithAccessoryListings = new[]
        {
            "bag",
            "body",
            "battery"
        };

        private static string[] _wordsAssociatedWithCameraListings = new[]
        {
            "mp",
            "megapixel",
            "zoom",
            "optical",
            "stabilized",
            "digitalkamera",
            "digital"
        };

        public bool ClassifyAsCamera(IDictionary<string, float> probablityPerToken, Listing listing)
        {
            // 1) Examine listing price

            // Score low cost listings (probability accessories) in proportion to how near zero they are.
            // TODO: Normalize price?
            const int LOW_COST = 75;
            var lowCostScore = (listing.Price < LOW_COST)
                ? ((1 - (listing.Price / LOW_COST)) * -100)
                : 0;

            var titleTokens = listing.Title.TokenizeOnWhiteSpace();

            // 2) Examine words in title.

            // Look for words typically associated with accessories
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

            // 3) Examine title sentence structure

            // Look for phrase "camera with {feature}"
            // TODO: Handle other languages. Ex: "Livré avec chargeur"
            var withWordIdx = titleTokens.LastIndexWhere(x => x == "with");
            var precededByCameraWord = (withWordIdx > 0 && titleTokens[withWordIdx - 1] == "camera");
            var phraseCameraWithFeatureScore = (withWordIdx > 0 && precededByCameraWord) ? 100 : 0;

            // Look for phrase "for {manufacturer name}" and "for {model}"
            // TODO: Check following word is a manufacturer name or product model
            var forWordIdx = titleTokens.LastIndexWhere(x => x == "for");
            var praseAccessoryForCameraScore = (forWordIdx > 0) ? -100 : 0;

            // Look for phrase "for {manufacturer/model}" with {feature}"
            // TODO: Check following word is a manufacturer name or product model
            var phraseAccessoryForCameraWithScore = (forWordIdx > 0 && withWordIdx > 0 && forWordIdx < withWordIdx) ? -100 : 0;

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
