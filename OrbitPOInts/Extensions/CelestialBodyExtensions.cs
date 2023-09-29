#if TEST
using UnityEngineMock;
using UnityEngineMock.JetBrains.Annotations;
using System.Linq;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
#else
using UnityEngine;
using JetBrains.Annotations;
using UniLinq;
using KSP_CelestialBody = CelestialBody;
using KSP_FlightGlobals = FlightGlobals;
#endif

namespace OrbitPOInts.Extensions
{
    using CelestialBody = KSP_CelestialBody;
    using FlightGlobals = KSP_FlightGlobals;

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
