using System;
using System.Collections.Generic;

namespace Pipeline.Analysis
{
    public class WordFrequencyPerCollection
    {
        public IDictionary<string, double> Count<T>(IEnumerable<T> docs, Func<T, String> selector)
        {
            var wordFrequency = new Dictionary<string, int>();
            var numDocs = 0;
            foreach(var doc in docs)
            {
                numDocs += 1;

                var line = selector(doc);
                var tokens = line.Split(null); // null splits based on Unicode Char.IsWhiteSpace

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