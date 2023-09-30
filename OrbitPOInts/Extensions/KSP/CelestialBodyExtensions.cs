using System;

#if TEST
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
using JB_Annotations = UnityEngineMock.JetBrains.Annotations;
#else
using UnityEngine;
using UniLinq;
using KSP_CelestialBody = CelestialBody;
using KSP_FlightGlobals = FlightGlobals;
using JB_Annotations = JetBrains.Annotations;
#endif

namespace OrbitPOInts.Extensions.KSP
{
    using CelestialBody = KSP_CelestialBody;
    using FlightGlobals = KSP_FlightGlobals;
    using CanBeNull = JB_Annotations.CanBeNullAttribute;

    public static class CelestialBodyExtensions
    {
        // TODO: scale sampleRes based on body.Radius
        public static double GetApproxTerrainMaxHeight(this CelestialBody body, int sampleResolution = 100)
        {
            var maxAltitude = Double.NegativeInfinity;

            for (var i = 0; i <= sampleResolution; i++)
            {
                for (var j = 0; j <= sampleResolution; j++)
                {
                    var latitude = (i / (double)sampleResolution) * 180 - 90;
                    var longitude = (j / (double)sampleResolution) * 360 - 180;

                    var altitude = body.TerrainAltitude(latitude, longitude, true);
                    maxAltitude = Math.Max(maxAltitude, altitude);
                }
            }

            return maxAltitude;
        }

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
