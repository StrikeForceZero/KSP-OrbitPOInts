using System.Collections.Generic;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;

#if TEST
using System.Linq;
using UnityEngineMock;
#else
using UniLinq;
using UnityEngine;
#endif


namespace OrbitPOInts.Data.POI
{
    using Logger = OrbitPOInts.Utils.Logger;

    public static class PoiUtils
    {
        public static IEnumerable<POI> GetPatchedPois(POI poi)
        {
            return poi.IsGlobal()
                ? FlightGlobals.Bodies.Select(body => PatchGlobalPoi(poi, body))
                : new List<POI> { poi };
        }

        public static POI PatchGlobalPoi(POI poi, CelestialBody body)
        {
            Logger.LogDebug($"patching global poi {Logger.GetPoiLogId(poi)} with body {body.Serialize()}");
            return poi.CloneWith(body, true);
        }
    }
}
