using System.Collections.Generic;
using OrbitPOInts.Utils;

namespace OrbitPOInts.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void AddKeyValuePair<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair)
        {
            dictionary.Add(pair.Key, pair.Value);
        }

        public static Option<V> TryGet<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            return dictionary.TryGetValue(key, out var value) ? new Option<V>(value) : new Option<V>();
        }

        public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }
    }
}
