using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pipeline.Infrastructure
{
    public static class StringExtensions
    {
        /// <summary>
        /// Splits based on Unicode Char.IsWhiteSpace
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] TokenizeOnWhiteSpace(this string str)
        {
            if (String.IsNullOrEmpty(str)) { return new string[0]; }

            return str.Split(null); // null splits based on Unicode Char.IsWhiteSpace
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateNGrams(this string tokens, int from, int to)
        {
            Debug.Assert(from > 0, "Expected min from value to be 1 uni-gram");
            for (var i = from; i <= to; i++)
                foreach (var ngram in CreateNGrams(tokens, i))
                    yield return ngram;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<string> CreateNGrams(this string tokens, int n)
        {
            var tmp = new List<char>();
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