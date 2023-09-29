using System.Collections.Generic;

namespace OrbitPOInts.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void AddKeyValuePair<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair)
        {
            dictionary.Add(pair.Key, pair.Value);
        }
    }
}
