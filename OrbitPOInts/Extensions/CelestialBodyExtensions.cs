using JetBrains.Annotations;

#if TEST
using KSP_CelestialBody = KSPMock.CelestialBody;
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_CelestialBody = CelestialBody;
#endif

namespace OrbitPOInts.Extensions
{
    using CelestialBody = KSP_CelestialBody;
    public static class CelestialBodyExtensions
    {
        public static bool TryDeserialize(string input, [CanBeNull] out CelestialBody result)
        {
            result = null;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var body in FlightGlobals.Bodies)
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
