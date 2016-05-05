using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pipeline.Infrastructure
{
    public static class IListDoubleExtensions
    {
        /// <summary>
        /// Finds the percentile value of the list.
        /// </summary>
        /// <param name="vals">Values to examine</param>
        /// <param name="percentile">Percentile to find</param>
        /// <returns></returns>
        public static double FindPercentile(this IList<double> vals, double percentile)
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

        /// <summary>
        /// Calculates the center of mass of the list using the array
        /// index as the distance from the origin.
        /// </summary>
        public static double CenterOfMassIndex(this IList<double> vals)
        {
            if (vals.Count <= 1) { return 0; }

            var sumOfMassDistance = 0D;
            var totalMass = 0D;
            for(var i = 0; i < vals.Count; i++)
            {
                sumOfMassDistance += (i * vals[i]);
                totalMass += vals[i];
            }

            return sumOfMassDistance / totalMass;
        }
    }
}
