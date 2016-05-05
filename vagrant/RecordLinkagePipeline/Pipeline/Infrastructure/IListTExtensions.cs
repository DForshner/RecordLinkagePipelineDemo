using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pipeline.Infrastructure
{
    public static class IListTExtensions
    {
        /// <summary>
        /// Find the index of the last element that meets the predicate.
        /// </summary>
        /// <returns>-1 if predicate not met by any element</returns>
        public static int LastIndexWhere<T>(this IList<T> vals, Func<T, bool> predicate)
        {
            var maxIdx = -1;
            for (var i = 0; i < vals.Count; i++)
            {
                if (predicate(vals[i]))
                    maxIdx = i;
            }
            return maxIdx;
        }
    }
}
