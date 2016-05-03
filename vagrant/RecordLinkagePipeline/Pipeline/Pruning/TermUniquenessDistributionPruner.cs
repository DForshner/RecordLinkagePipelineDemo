using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Analysis;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Pruning
{
    /// <summary>
    /// Tries to determine if listing is an accessory based on the distribution of distribution of its term probabilities.
    /// Prototype: https://github.com/DForshner/CSharpExperiments/blob/master/ClassifyingDocumentsUsingDistributionOfTermUniqueness.cs
    /// </summary>
    public class TermUniquenessDistributionPruner : IListingPruner
    {
        private const int NUM_BUCKETS = 30;
        private static IList<double> _axisValues = GenerateHistogramAxis();

        /// <summary>
        /// TODO: ok for virtual dispatch per listing?
        /// </summary>
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
            if (histogram.Count < 2)
            {
                return true;
            }

            var medianIdx = 0.5F * (float)histogram.Count;

            var left = 0F;
            var right = 0F;
            for (var i = 0; i < histogram.Count; i++)
            {
                var dist = Math.Abs(i - medianIdx);
                if (i < medianIdx)
                    left += dist * histogram[i];
                else if (i >= medianIdx)
                    right += dist * histogram[i];
            }

            return false;
        }

        /// <summary>
        /// Assign this term probabilities to a histogram
        /// </summary>
        private static IList<int> CreateTermProbabilityHistrogram(IList<double> probablitities)
        {
            var minProbability = probablitities.Min();
            var numBucketsNeeded = 0;
            while (numBucketsNeeded < NUM_BUCKETS && _axisValues[numBucketsNeeded] >= minProbability)
            {
                numBucketsNeeded += 1;
            }

            var histogram = Enumerable.Repeat(0, numBucketsNeeded + 1).ToList();
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