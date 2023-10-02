using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace OrbitPOInts.Tests.CustomAsserts
{
    public static class CollectionAssert
    {
        public static void HasCountN<T>(IEnumerable<T> collection, int expectedCount, string message = "")
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "Collection cannot be null.");
            }

            var actualCount = collection.Count();

            if(actualCount != expectedCount)
            {
                if (message.Length > 0) message += "\n";
                throw new AssertionException($"{message}Expected count: {expectedCount}, but was: {actualCount}.");
            }
        }

        public static void HaveSameElements<T>(
            IEnumerable<T> collection1,
            IEnumerable<T> collection2,
            IEqualityComparer<T> comparer = null
        ) => HaveSameElements(collection1, collection2, null, comparer);

        public static void HaveSameElements<T>(
            IEnumerable<T> collection1,
            IEnumerable<T> collection2,
            Func<T, string> serializer = null,
            IEqualityComparer<T> comparer = null
        )
        {
            comparer ??= EqualityComparer<T>.Default;

            if (collection1.Count() != collection2.Count() || !collection1.SequenceEqual(collection2, comparer))
            {
                AssertCollectionsAreEquivalent(collection1, collection2, serializer, comparer);
            }
        }

        public static void AssertCollectionsAreEquivalent<T>(
            IEnumerable<T> collection1,
            IEnumerable<T> collection2,
            IEqualityComparer<T> comparer = null
        ) => AssertCollectionsAreEquivalent(collection1, collection2, null, comparer);


        public static void AssertCollectionsAreEquivalent<T>(
            IEnumerable<T> collection1,
            IEnumerable<T> collection2,
            Func<T, string> serializer = null,
            IEqualityComparer<T> comparer = null
        )
        {
            comparer ??= EqualityComparer<T>.Default;
            serializer ??= t => t.ToString();

            var collection1Counts = collection1.GroupBy(x => x, comparer).ToDictionary(g => g.Key, g => g.Count(), comparer);
            var collection2Counts = collection2.GroupBy(x => x, comparer).ToDictionary(g => g.Key, g => g.Count(), comparer);

            var sameCounts = collection1Counts.Count == collection2Counts.Count;
            var areEquivalent = sameCounts && collection1Counts.All(kv => collection2Counts.TryGetValue(kv.Key, out var count) && count == kv.Value);

            if (areEquivalent) return;

            var message = "The collections do not have the same elements.\n";

            foreach (var kv in collection1Counts)
            {
                if (!collection2Counts.TryGetValue(kv.Key, out var countInSecond))
                {
                    message += $"Element {serializer.Invoke(kv.Key)} is missing in the second collection. It occurs {kv.Value} time(s) in the first collection.\n";
                }
                else if (kv.Value != countInSecond)
                {
                    message += $"Element {serializer.Invoke(kv.Key)} has different occurrences. It occurs {kv.Value} time(s) in the first collection and {countInSecond} time(s) in the second collection.\n";
                }
            }

            foreach (var kv in collection2Counts)
            {
                if (!collection1Counts.ContainsKey(kv.Key))
                {
                    message += $"Element {serializer.Invoke(kv.Key)} is extra in the second collection. It occurs {kv.Value} time(s) in the second collection.\n";
                }
            }

            throw new AssertionException(message);
        }


    }

}
