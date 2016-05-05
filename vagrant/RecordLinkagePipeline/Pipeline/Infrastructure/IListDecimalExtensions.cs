using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pipeline.Infrastructure
{
    public static class IListDecimalExtensions
    {
        /// <summary>
        /// Returns the interquartile weak outlier range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<decimal, decimal> InterquartileWeakOutlierRange(this IList<Decimal> vals)
        {
            const decimal WEAK_OUTLIER = 1.5M;
            return CalculateInterquartileRange(vals, WEAK_OUTLIER);
        }

        /// <summary>
        /// Returns the interquartile strong outlier range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<decimal, decimal> InterquartileStrongOutlierRange(this IList<Decimal> vals)
        {
            const decimal STRONG_OUTLIER = 3M;
            return CalculateInterquartileRange(vals, STRONG_OUTLIER);
        }

        /// <summary>
        /// http://statistics.about.com/od/Descriptive-Statistics/a/How-Do-We-Determine-What-Is-An-Outlier.htm
        /// </summary>
        private static Tuple<decimal, decimal> CalculateInterquartileRange(IList<Decimal> vals, decimal outlierMultiplier)
        {
            var firstQuartileIdx = (int)((float)vals.Count * 0.25F);
            var firstQuartile = vals[firstQuartileIdx];

            var thirdQuartileIdx = (int)((float)vals.Count * 0.75F);
            var thirdQuartile = vals[thirdQuartileIdx];

            var delta = Math.Abs(thirdQuartile - firstQuartile) * outlierMultiplier;
            return Tuple.Create(firstQuartile - delta, thirdQuartile + delta);
        }

        /// <summary>
        /// σ = sqrt [ Σ ( Xi - X )2 / N ]
        /// </summary>
        /// <param name="vals">values</param>
        /// <returns>σ</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal StdDeviation(this ICollection<Decimal> vals)
        {
            if (vals.Count == 0M) { return 0M; }
            var avg = vals.Average();

            // TODO: Get rid of the casts to double so this isn't tossing away precision
            var sum = vals.Sum(x => Math.Pow((double)x - (double)avg, 2));
            return (decimal)Math.Sqrt(sum / (vals.Count - 1));
        }
    }
}
