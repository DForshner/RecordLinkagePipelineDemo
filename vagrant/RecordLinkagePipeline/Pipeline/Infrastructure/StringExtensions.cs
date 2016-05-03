using System.Collections.Generic;
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
            return str.Split(null); // null splits based on Unicode Char.IsWhiteSpace
        }

        /// <summary>
        /// Generates character n-grams
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateBiTriQuadCharacterNGrams(this string str)
        {
            for (var i = 0; i < str.Length - 1; i++)
            {
                yield return new string(new char[] { str[i], str[i + 1] });
            }

            for (var i = 0; i < str.Length - 2; i++)
            {
                yield return new string(new char[] { str[i], str[i + 1], str[i + 2] });
            }

            for (var i = 0; i < str.Length - 3; i++)
            {
                yield return new string(new char[] { str[i], str[i + 1], str[i + 2], str[i + 3] });
            }
        }

        /// <summary>
        /// Generates word/token shingles
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> CreateUniBiTokenShingles(this string str)
        {
            var tokens = str.Split(null); // null splits based on Unicode Char.IsWhiteSpace

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
    }
}