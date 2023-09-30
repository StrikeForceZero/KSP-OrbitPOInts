using System.Collections.Generic;

namespace OrbitPOInts.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(int Index, T Value)> WithIndex<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return (i, list[i]);
            }
        }
    }
}
