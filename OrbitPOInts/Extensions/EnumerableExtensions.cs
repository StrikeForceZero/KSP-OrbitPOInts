using System.Collections.Generic;

namespace OrbitPOInts.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(int Index, T Value)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((t, i) => (i, t));
        }
    }
}
