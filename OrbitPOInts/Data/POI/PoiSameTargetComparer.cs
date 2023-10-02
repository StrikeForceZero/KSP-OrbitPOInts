using System.Collections.Generic;

namespace OrbitPOInts.Data.POI
{
    public class PoiSameTargetComparer : IEqualityComparer<POI>
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
            return PoiDTOSameTargetComparer.StaticEquals(PoiDTO.FromPoi(x), PoiDTO.FromPoi(y));
        }

        public static int StaticGetHashCode(POI obj)
        {
            if (obj == null) return 0;
            return PoiDTOSameTargetComparer.StaticGetHashCode(PoiDTO.FromPoi(obj));
        }
    }

}
