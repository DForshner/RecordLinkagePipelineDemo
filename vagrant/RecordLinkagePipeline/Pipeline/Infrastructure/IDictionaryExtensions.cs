using System;
using System.Collections.Generic;

namespace Pipeline.Infrastructure
{
    public static class IDictionaryTExtensions
    {
        /// <summary>
        /// Add or increment the counter associated with the key.
        /// </summary>
        public static void AddOrIncrement<K>(this IDictionary<K, int> toUpdate, K key)
        {
            if (toUpdate.ContainsKey(key))
                toUpdate[key]++;
            else
                toUpdate.Add(key, 1);
        }
    }
}