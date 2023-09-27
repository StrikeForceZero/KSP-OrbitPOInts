using System.Collections.Generic;

namespace OrbitPOInts.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddKeyValuePair<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair)
        {
            dictionary.Add(pair.Key, pair.Value);
        }
    }
}
