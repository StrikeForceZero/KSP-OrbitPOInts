using System.Collections.Generic;
#if TEST
using UnityEngineMock;
using KSP_ConfigNode = KSPMock.ConfigNode;
using KSP_CelestialBody = KSPMock.CelestialBody;

#else
using UniLinq;
using UnityEngine;
using KSP_ConfigNode = ConfigNode;
using KSP_CelestialBody = CelestialBody;
#endif

namespace OrbitPOInts.Data.POI
{
    using CelestialBody = KSP_CelestialBody;
    using ConfigNode = KSP_ConfigNode;

    public class PoiComparer : IEqualityComparer<POI>
    {
        public bool Equals(POI x, POI y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(POI obj)
        {
            return StaticGetHashCode(obj);
        }

        public static bool StaticEquals(POI x, POI y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return PoiDTOComparer.StaticEquals(PoiDTO.FromPoi(x), PoiDTO.FromPoi(y));
        }

        public static int StaticGetHashCode(POI obj)
        {
            if (obj == null) return 0;
            return PoiDTOComparer.StaticGetHashCode(PoiDTO.FromPoi(obj));
        }
    }

}
