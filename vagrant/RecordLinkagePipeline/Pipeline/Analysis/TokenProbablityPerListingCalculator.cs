using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Analysis
{
    /// <summary>
    /// Get title term probability over all listings(docs)
    /// TODO: This could be more generic
    /// </summary>
    internal static class TokenProbablityPerListingCalculator
    {
        public static Dictionary<string, float> GenerateTokenProbabilitiesPerListing(ICollection<Listing> listings)
        {
            var freqByToken = new ConcurrentDictionary<string, int>();
            var docCount = 0;
            listings
                .AsParallel()
                .ForAll(x =>
                {
                    foreach (var token in x.Title.TokenizeOnWhiteSpace())
                    {
                        freqByToken.AddOrUpdate(token, 1, (key, val) => { return val + 1; });
                    }

                    Interlocked.Increment(ref docCount);
                });

            // Turn freq into probability per doc
            return freqByToken.ToDictionary(x => x.Key, x => (float)x.Value / docCount);
        }
    }
}