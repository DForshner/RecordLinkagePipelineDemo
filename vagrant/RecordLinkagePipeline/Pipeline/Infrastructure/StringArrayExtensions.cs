using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pipeline.Infrastructure
{
    public static class StringArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateNShingles(this string[] tokens, int from, int to)
        {
            Debug.Assert(from > 0, "Expected min from value to be 1 uni-gram");
            for(var i = from; i <= to; i++)
                foreach (var ngram in CreateNShingles(tokens, i))
                    yield return ngram;
        }

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
    }
}