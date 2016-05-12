﻿using System.Collections.Generic;
using Pipeline.Infrastructure;

namespace Pipeline.Analysis
{
    public static class WordFrequencyCalculator
    {
        public static IDictionary<string, int> GetWordFrequency(IEnumerable<string> docs)
        {
            var freq = new Dictionary<string, int>();
            foreach (var doc in docs)
            {
                var tokens = doc.TokenizeOnWhiteSpace();
                foreach (var token in tokens)
                {
                    if (!freq.ContainsKey(token)) { freq.Add(token, 0); }
                    freq[token] += 1;
                }
            }
            return freq;
        }
    }
}
