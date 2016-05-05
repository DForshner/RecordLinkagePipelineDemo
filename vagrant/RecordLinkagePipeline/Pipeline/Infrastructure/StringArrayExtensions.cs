using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
            for (var i = 0; i < tokens.Length - 1; i++)
            {
                yield return tokens[i];
                yield return tokens[i] + tokens[i + 1];
            }

            if (tokens.Length > 0)
            {
                yield return tokens[tokens.Length - 1];
            }
        }

        /// <summary>
        /// Generates word/token shingles
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateBiTriTokenShingles(this string[] tokens)
        {
            for (var i = 0; i < tokens.Length - 1; i++)
            {
                yield return tokens[i] + tokens[i + 1];
            }
            for (var i = 0; i < tokens.Length - 2; i++)
            {
                yield return tokens[i] + tokens[i + 1] + tokens[i + 2];
            }
        }
    }
}