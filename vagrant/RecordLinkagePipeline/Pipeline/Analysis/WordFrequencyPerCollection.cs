using System;
using System.Collections.Generic;
using Pipeline.Infrastructure;

namespace Pipeline.Analysis
{
    internal class WordFrequencyPerCollection
    {
        public IDictionary<string, double> Count<T>(IEnumerable<T> docs, Func<T, String> selector)
        {
            var wordFrequency = new Dictionary<string, int>();
            var numDocs = 0;
            foreach(var doc in docs)
            {
                numDocs += 1;

                var line = selector(doc);
                var tokens = line.TokenizeOnWhiteSpace();

                var uniqueWordsPerDoc = new HashSet<string>();
                foreach (var token in tokens)
                {
                    if (!uniqueWordsPerDoc.Contains(token))
                    {
                        uniqueWordsPerDoc.Add(token);
                    }
                }

                foreach(var word in uniqueWordsPerDoc)
                {
                    if (!wordFrequency.ContainsKey(word))
                    {
                        wordFrequency.Add(word, 0);
                    }
                    wordFrequency[word] += 1;
                }
            }

            var inverseDocFrequency = new Dictionary<string, double>();
            foreach(var term in wordFrequency)
            {
                inverseDocFrequency.Add(term.Key, Math.Log((double)numDocs / (double)term.Value));
            }

            return inverseDocFrequency;
        }
    }
}