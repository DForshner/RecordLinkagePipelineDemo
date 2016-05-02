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
    }
}