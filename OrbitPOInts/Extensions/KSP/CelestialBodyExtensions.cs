using System;

#if TEST
using System.Linq;
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

        // TODO: technically this isn't an extension method and probably belongs in a utils/helper class
        public static bool TryDeserialize(string input, [CanBeNull] out CelestialBody result)
        {
            result = CelestialBodyCache.Instance.ResolveByName(input);
            return result != null;
        }

        [CanBeNull]
        public static string Serialize([CanBeNull] this CelestialBody body)
        {
            // ReSharper disable once Unity.NoNullPropagation
            return body?.name;
        }
    }
}
