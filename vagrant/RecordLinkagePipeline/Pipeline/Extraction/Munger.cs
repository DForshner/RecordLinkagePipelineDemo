using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    internal static class Munger
    {
        /// <summary>
        /// Remove punctuation, multiple spaces in a row, line endings, and lowercase everything
        /// </summary>
        public static string Munge(string original)
        {
            var temp = original.ToCharArray();
            var result = new List<char>(original.Length);
            bool lastCharWasSpace = false;
            foreach (var c in temp)
            {
                if (char.IsLetter(c) || char.IsNumber(c))
                {
                    result.Add(char.ToLower(c));
                    lastCharWasSpace = false;
                }
                else if (!lastCharWasSpace)
                {
                    result.Add(' ');
                    lastCharWasSpace = true;
                }
            }
            return new String(result.ToArray());
        }
    }
}
