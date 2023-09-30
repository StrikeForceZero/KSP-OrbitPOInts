using System;
using System.Collections.Generic;

#if TEST
using System.Linq;
#else
using UniLinq;
#endif

namespace OrbitPOInts.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(int Index, T Value)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((t, i) => (i, t));
        }

        public static TCollection AddAll<TCollection, T>(this TCollection collection, params T[] values) where TCollection : ICollection<T>
        {
            Array.ForEach(values, collection.Add);
            return collection;
        }
    }
}
