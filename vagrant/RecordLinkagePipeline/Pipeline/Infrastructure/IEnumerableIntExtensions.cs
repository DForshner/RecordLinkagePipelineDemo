using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pipeline.Infrastructure
{
    public static class IEnumerableIntExtensions
    {
        public static float CalculateEntropy(this IEnumerable<int> frequencies, int total)
        {
            var h = (-1D) * frequencies
                .Where(f => f <= total)
                .Select(f => (double)f / total) // probabilities
                .Select(p => p * Math.Log(p, 2))
                .Sum();
            return (float)h;
        }
    }
}