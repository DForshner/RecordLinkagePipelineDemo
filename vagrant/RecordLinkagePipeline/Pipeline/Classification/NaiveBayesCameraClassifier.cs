using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Classification
{
    /// <summary>
    /// Classifies listings as cameras uses a Naive Bayes classifier.
    ///
    /// Prototype: https://github.com/DForshner/CSharpExperiments/blob/master/NaiveBayesCameraListingClassifier.cs
    /// </summary>
    internal class NaiveBayesCameraClassifier
    {
        private readonly ICollection<string> _cameraTrainingSet;
        private readonly ICollection<string> _accessoryTrainingSet;
        private readonly int _minNumWords;
        private readonly float _wordRatio;
        private readonly IDictionary<string, int> _cameraWordFreq;
        private readonly IDictionary<string, int> _accessoryWordFreq;
        private readonly int _totalTerms;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cameraTrainingSet"></param>
        /// <param name="accessoryTrainingSet"></param>
        /// <param name="minNumWords">The minimum number of non-camera terms required to classify as non-camera.</param>
        /// <param name="wordRatio">The ratio of camera to non-camera terms to classify as camera.</param>
        public NaiveBayesCameraClassifier(ICollection<string> cameraTrainingSet, ICollection<string> accessoryTrainingSet, int minNumWords, float wordRatio)
        {
            Debug.Assert(cameraTrainingSet != null, "expected cameraTrainingSet not null");
            Debug.Assert(accessoryTrainingSet != null, "expected accessoryTrainingSet not null");
            if (minNumWords < 0) { throw new ArgumentOutOfRangeException("minNumWords"); }
            if (wordRatio < 0 || wordRatio > 1) { throw new ArgumentOutOfRangeException("wordRatio"); }

            _cameraTrainingSet = cameraTrainingSet;
            _accessoryTrainingSet = accessoryTrainingSet;
            _minNumWords = minNumWords;
            _wordRatio = wordRatio;
            _cameraWordFreq = GetWordFrequency(cameraTrainingSet);
            _accessoryWordFreq = GetWordFrequency(accessoryTrainingSet);
            _totalTerms = _cameraWordFreq.Values.Sum() + _accessoryWordFreq.Values.Sum();
        }

        public bool IsCamera(Listing listing)
        {
            Debug.Assert(listing != null, "expected listing not null");

            var cameraTermCount = 0;
            var nonCameraTermCount = 0;
            var tokens = listing.Title.TokenizeOnWhiteSpace();
            foreach (var token in tokens)
            {
                float tokenQ = CalculateQ(token);
                if (tokenQ > 1F)
                {
                    cameraTermCount += 1;
                }
                else if (tokenQ < 1F)
                {
                    nonCameraTermCount += 1;
                }
                // else neutral words are ignored
            }

            var totalTerms = nonCameraTermCount + cameraTermCount;
            return
                // If not enough accessory words assume it is a camera
                (nonCameraTermCount < _minNumWords)

                // If not enough accessory words assume it is a camera
                || ((float)cameraTermCount / totalTerms > _wordRatio);
        }

        private float CalculateQ(string token)
        {
            var numCameraListings = _cameraTrainingSet.Count;
            float probWordOccuringInCamera = _cameraWordFreq.ContainsKey(token) ? (float)_cameraWordFreq[token] / numCameraListings : 0F;
            Debug.Assert(probWordOccuringInCamera <= 1.0F, "Expected probability range [0 - 1.0]");

            var numAccessoryListings = _accessoryTrainingSet.Count;
            float probWordOccuringInAccessory = _accessoryWordFreq.ContainsKey(token) ? (float)_accessoryWordFreq[token] / numAccessoryListings : 0F;
            Debug.Assert(probWordOccuringInAccessory <= 1.0F, "Expected probability range [0 - 1.0]");

            int tokenOccurances = (_cameraWordFreq.ContainsKey(token) ? _cameraWordFreq[token] : 0) + (_accessoryWordFreq.ContainsKey(token) ? _accessoryWordFreq[token] : 0);
            float probTokenOccuring = (float)tokenOccurances / _totalTerms;

            if (probTokenOccuring.IsNearZero()) { return 1; }

            float probCameraListing = (float)numCameraListings / (numCameraListings + numAccessoryListings);
            float probAccessoryListing = (float)numAccessoryListings / (numCameraListings + numAccessoryListings); ;

            // Use Bayes' theorem to get probability this is camera or accessory given token
            // P(camera|token) = P(token|camera) * P(camera) / P(token)
            float probCameraGivenToken = (probWordOccuringInCamera * probCameraListing) / probTokenOccuring;

            float probAccessoryGivenToken = (probWordOccuringInAccessory * probAccessoryListing) / probTokenOccuring;

            // Q ratio to tells us how likely this a camera listing given the token occurred
            return !probAccessoryGivenToken.IsNearZero() ? (probCameraGivenToken / probAccessoryGivenToken) : float.PositiveInfinity;
        }

        private static IDictionary<string, int> GetWordFrequency(IEnumerable<string> docs)
        {
            var freq = new Dictionary<string, int>();
            foreach (var doc in docs)
            {
                var tokens = doc.TokenizeOnWhiteSpace();
                foreach (var token in tokens)
                {
                    if (!freq.ContainsKey(token)) { freq.Add(token, 0); }
                    freq[token] += 1;
                }
            }
            return freq;
        }
    }
}