using System.Collections.Generic;
using OrbitPOInts.Extensions;

namespace OrbitPOInts.Data.POI
{
    public class PoiDTOComparer : IEqualityComparer<PoiDTO>
    {

        public bool Equals(PoiDTO x, PoiDTO y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(PoiDTO obj)
        {
            return StaticGetHashCode(obj);
        }

        public static bool StaticEquals(PoiDTO x, PoiDTO y)
        {
            if (ReferenceEquals(x, y)) return true; // Fast reference equality check.

            if (x == null || y == null) return false; // Quick null check.

            // The conditions are ordered by the likelihood of them being different and
            // their computational cost, from least to most expensive.
            return x.Type == y.Type && // Simple enum comparison, most likely to be different.
                   ReferenceEquals(x.Body, y.Body) && // Reference comparison, likely to be different.
                   x.Radius.AreRelativelyEqual(y.Radius) && // ~7 operations, likely to be different.
                   x.Enabled == y.Enabled &&
                   x.Label == y.Label &&
                   x.Color.Equals(y.Color) && // 4 float comparisons.
                   x.AddPlanetRadius == y.AddPlanetRadius &&
                   x.Resolution == y.Resolution &&
                   x.LineWidth.AreRelativelyEqual(y.LineWidth); // ~7 operations.
        }

        public static int StaticGetHashCode(PoiDTO obj)
        {
            if (obj == null) return 0;

            var hash = 17; // Prime number to start with.
            hash = hash * 31 + (obj.Label?.GetHashCode() ?? 0);
            hash = hash * 31 + obj.Enabled.GetHashCode();
            hash = hash * 31 + obj.Radius.GetHashCode();
            hash = hash * 31 + obj.Color.GetHashCode();
            hash = hash * 31 + obj.Type.GetHashCode();
            // CelestialBody will probably outlive the lifetime of the mod
            // ReSharper disable once Unity.NoNullPropagation
            hash = hash * 31 + (obj.Body?.GetHashCode() ?? 0);
            hash = hash * 31 + obj.AddPlanetRadius.GetHashCode();
            hash = hash * 31 + obj.LineWidth.GetHashCode();
            hash = hash * 31 + obj.Resolution.GetHashCode();

            return hash;
        }
    }
}