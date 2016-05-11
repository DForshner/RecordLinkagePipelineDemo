using System;
using System.Collections.Generic;

namespace Pipeline.Infrastructure
{
    public static class IEnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
        {
            return new HashSet<T>(list);
        }
    }
}