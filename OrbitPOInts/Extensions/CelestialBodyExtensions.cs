using JetBrains.Annotations;
using OrbitPOInts.Wrappers;

#if TEST
using CelestialBody = KSPMock.CelestialBody;
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.Extensions
{
    public static class CelestialBodyExtensions
    {
        public static bool TryDeserialize(string input, [CanBeNull] out CelestialBody result)
        {
            result = null;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var body in FlightGlobalsWrapper.Instance.Bodies)
            {
                if (body.Serialize() != input) continue;
                result = body;
                return true;
            }
            return false;
        }

        [CanBeNull]
        public static string Serialize([CanBeNull] this CelestialBody body)
        {
            // ReSharper disable once Unity.NoNullPropagation
            return body?.name;
        }
    }
}
