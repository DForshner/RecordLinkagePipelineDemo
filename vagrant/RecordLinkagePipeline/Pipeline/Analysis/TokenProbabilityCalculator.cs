using System.Collections.Generic;
using System.Linq;
using Pipeline.Infrastructure;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Get term probability over all docs.
    /// TODO: This duplicates WordFrequencyCalculator
    /// </summary>
    internal static class TokenProbabilityCalculator
    {
        public static Dictionary<string, float> GetProbabilities(IEnumerable<string> docs)
        {
            var freq = new Dictionary<string, int>();
            var numDocs = 0;
            foreach(var doc in docs)
            {
                var tokens = doc.TokenizeOnWhiteSpace();
                foreach (var token in tokens)
                {
                    freq.AddOrIncrement(token);
                }
                numDocs++;
            }

            // Turn freq into probability per doc
            return freq.ToDictionary(x => x.Key, x => (float)x.Value / numDocs);
        }
    }
}