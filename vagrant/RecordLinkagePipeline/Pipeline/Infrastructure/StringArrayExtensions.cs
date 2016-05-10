using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics;

namespace Pipeline.Infrastructure
{
    public static class StringArrayExtensions
    {
        /// <summary>
        /// Generates word/token shingles
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateUniBiTokenShingles(this string[] tokens)
        {
            return CreateNGrams(tokens, 1, 2);
        }

        /// <summary>
        /// Generates word/token shingles
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateBiTriTokenShingles(this string[] tokens)
        {
            return CreateNGrams(tokens, 2, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateNGrams(this string[] tokens, int from, int to)
        {
            Debug.Assert(from > 0, "Expected min from value to be 1 uni-gram");
            for(var i = from; i <= to; i++)
                foreach (var ngram in CreateNGrams(tokens, i))
                    yield return ngram;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateNGrams(this string[] tokens, int n)
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
    }
}