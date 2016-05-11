using System.Collections.Generic;
using System.Linq;
using Pipeline.Domain;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Classification
{
    /// <summary>
    /// Note: I couldn't get this classifier to work reliably so it's only included for reference.  I think the basic idea is sound and given more time I could probably get it working.
    ///
    /// Classifies a listing using the distribution of its term probabilities. The idea is to identify listings that are an accessory for multiple models by looking for listings with a high proportion of unique terms.
    /// Ex: nikon 85mm f 3 5 g vr af s dx ed micro nikkor lens uv filter accessory kit for nikon d300s d300 d40 d60 d5000 d7000 d90 d3000 d3100 digital slr cameras
    /// Assuming a typical camera listing with one model number only has a few unique terms it should be possible to look a histogram of term probabilities and spot the accessory listings.
    ///
    /// Prototype: https://github.com/DForshner/CSharpExperiments/blob/master/ClassifyingDocumentsUsingDistributionOfTermUniqueness.cs
    /// </summary>
    internal class TermUniquenessDistributionClassifier
    {
        /// <summary>
        /// Number of histogram buckets to assign term probabilities to.
        /// </summary>
        private const int NUM_BUCKETS = 15;

        /// <summary>
        /// The ratio of the center of mass to the range of buckets that have values.
        /// </summary>
        private const double CLASSIFICATION_RATIO = 0.33D;

        private static IList<double> _axisValues = GenerateHistogramAxis();

        public bool ClassifyAsCamera(IDictionary<string, float> probablityPerToken, Listing listing)
        {
            var probablities = GetProbabilitiesPerToken(probablityPerToken, listing);

            var histogram = CreateTermProbabilityHistrogram(probablities);

            return IsCamera(histogram);
        }

        private static IList<double> GenerateHistogramAxis()
        {
            var histogram = Enumerable.Repeat(0D, NUM_BUCKETS).ToList();

            // Most of the term probabilities are very small so we use buckets with increasingly smaller range.
            var prob = 1D; // 100%
            for (var i = 0; i < histogram.Count; i++)
            {
                // Each bucket is 1/2 range of previous.
                prob /= 2; // 0.5, 0.25, 0.0125, ...
                histogram[i] = prob;
            }
            return histogram;
        }

        private IList<double> GetProbabilitiesPerToken(IDictionary<string, float> probablityPerToken, Listing listing)
        {
            var probablities = new List<double>();
            foreach (var token in listing.Title.TokenizeOnWhiteSpace())
            {
                var prob = (double)probablityPerToken[token];
                probablities.Add(prob);
            }
            return probablities;
        }

        private static bool IsCamera(IList<int> histogram)
        {
            var maxFilled = histogram.LastIndexWhere(x => x > 0);

            const int MIN_NUM_BUCKETS = 2;
            if (maxFilled < MIN_NUM_BUCKETS)
            {
                // Not enough buckets filled to get a good classification.
                return true;
            }

            var center = histogram.Select(x => (double)x).ToList().CenterOfMassIndex();
            var ratio = center / maxFilled;

            return (ratio < CLASSIFICATION_RATIO);
        }

        /// <summary>
        /// Assign this term probabilities to a histogram
        /// </summary>
        private static IList<int> CreateTermProbabilityHistrogram(IList<double> probablitities)
        {
            var histogram = Enumerable.Repeat(0, NUM_BUCKETS).ToList();
            for (var i = 0; i < probablitities.Count; i++)
            {
                // Try each bucket until probability fits in range
                for (var j = 0; j < histogram.Count; j++)
                {
                    if (_axisValues[j] <= probablitities[i])
                    {
                        histogram[(int)j]++;
                        break;
                    }
                }
            }

            return histogram;
        }
    }
}