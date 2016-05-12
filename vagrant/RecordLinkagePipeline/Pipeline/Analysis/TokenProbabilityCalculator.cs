using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Get term probability over all docs.
    /// </summary>
    internal static class TokenProbabilityCalculator
    {
        public static IDictionary<string, float> GetProbabilities<T>(IEnumerable<T> docs, Func<T, string> selector)
        {
            var freqByToken = new ConcurrentDictionary<string, int>();
            var numDocs = 0;
            docs.AsParallel()
                .ForAll(x =>
                {
                    var tokens = selector(x).TokenizeOnWhiteSpace();
                    foreach (var token in tokens)
                    {
                        freqByToken.AddOrUpdate(token, 1, (key, val) => { return val + 1; });
                    }

                    Interlocked.Increment(ref numDocs);
                });

            // Turn freq into probability per doc
            return freqByToken.ToDictionary(x => x.Key, x => (float)x.Value / numDocs);
        }
    }
}