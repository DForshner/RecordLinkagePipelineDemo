using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pipeline.Infrastructure
{
    public static class StringArrayExtensions
    {
        /// <summary>
        /// Create shingles (word n-grams) from an array of strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateNShingles(this string[] tokens, int from, int to)
        {
            Debug.Assert(from > 0, "Expected min from value to be 1 uni-gram");
            for(var i = from; i <= to; i++)
                foreach (var ngram in CreateNShingles(tokens, i))
                    yield return ngram;
        }

        /// <summary>
        /// Create shingles (word n-grams) from an array of strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateNShingles(this string[] tokens, int n)
        {
            var tmp = new List<string>();
            for (var i = 0; i < tokens.Length - (n - 1); i++)
            {
                tmp.Clear();
                for (var j = 0; j < n; j++)
                {
                    tmp.Add(tokens[i + j]);
                }
                yield return string.Concat(tmp);
            }
        }

        /// <summary>
        /// Returns index of first element where predicate is true.
        /// </summary>
        /// <returns>-1 if no element matches predicate</returns>
        public static int FindIndex(this string[] arr, Func<string, bool> predicate)
        {
            for (var i = 0; i < arr.Length; i++)
            {
                if (predicate(arr[i]))
                    return i;
            }
            return -1; // Not found
        }
    }
}