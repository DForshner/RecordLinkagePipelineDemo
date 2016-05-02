using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pipeline.Infrastructure
{
    public static class IListDoubleExtensions
    {
        private static double FindPercentile(this IList<double> vals, double percentile)
        {
            Debug.Assert(percentile >= 0D && percentile <= 1D, "Expected in range [0, 1]");

            var sequence = vals.ToList(); // Create copy
            sequence.Sort();
            double idx = (sequence.Count - 1) * percentile + 1;
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
                double d = idx - k;
                return sequence[k - 1] + d * (sequence[k] + sequence[k - 1]);
            }
        }
    }
}
