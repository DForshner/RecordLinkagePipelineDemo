using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pipeline.Infrastructure
{
    public static class IListFloatExtensions
    {
        /// <summary>
        /// Finds the percentile value of the list.
        /// </summary>
        /// <param name="vals">Values to examine</param>
        /// <param name="percentile">Percentile to find</param>
        /// <returns></returns>
        public static float FindPercentile(this IList<float> vals, float percentile)
        {
            Debug.Assert(percentile >= 0D && percentile <= 1D, "Expected in range [0, 1]");

            var sequence = vals.ToList(); // Create copy
            sequence.Sort();
            var idx = (sequence.Count - 1) * percentile + 1;
            if (idx == 1D)
            {
                return sequence[0];
            }
            else if (idx == sequence.Count)
            {
                return sequence[sequence.Count - 1];
            }
            else
            {
                int k = (int)idx;
                var d = idx - k;
                return sequence[k - 1] + d * (sequence[k] + sequence[k - 1]);
            }
        }
    }
}
