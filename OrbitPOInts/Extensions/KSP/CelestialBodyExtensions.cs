using System;

#if TEST
using System.Linq;
using UnityEngineMock;
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
        public static bool IsPqsUsable(this CelestialBody body) =>
            body && body.pqsController && body.pqsController.isActiveAndEnabled;
        
        // TODO: scale sampleRes based on body.Radius
        public static double GetApproxTerrainMaxHeight(this CelestialBody body, int sampleResolution = 100)
        {
            if (!body) return 0;

            if (!body.IsPqsUsable())
            {
                Debug.LogError($"[OrbitPOInts] Failed GetApproxTerrainMaxHeight {body?.bodyName ?? "<null>"}: PQS not ready");
                return 0;
            }

            var pqs = body.pqsController;

            double maxAlt = 0;
            for (var i = 0; i < sampleResolution; i++)
            {
                var lat = -90 + 180.0 * (i / (double)(sampleResolution - 1));
                for (var j = 0; j < sampleResolution; j++)
                {
                    var lon = -180 + 360.0 * (j / (double)(sampleResolution - 1));
                    try
                    {
                        // Avoid CelestialBody.TerrainAltitude
                        // go straight to PQS and override the quad check.
                        var radial = body.GetRelSurfaceNVector(lat, lon) * body.Radius;
                        var surface = pqs.GetSurfaceHeight(radial);
                        var alt = surface - body.Radius;
                        maxAlt = Math.Max(maxAlt, alt);
                    }
                    catch (Exception ex)
                    {
                        // Ignore individual bad samples (mods can throw during init)
                        Debug.LogError($"[OrbitPOInts] Failed sampling {body?.bodyName ?? "<null>"}: {ex}");
                    }
                }
            }
            return Math.Max(0, maxAlt);
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
