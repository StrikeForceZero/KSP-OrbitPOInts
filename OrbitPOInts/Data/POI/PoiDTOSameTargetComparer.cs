using System.Collections.Generic;
using OrbitPOInts.Extensions;
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

    // TODO: use reflection and attributes to handle all props instead of writing out each one

    // This comparator targets checking equality between Type, Body, and Radius if Type=Custom/None
    public class PoiDTOSameTargetComparer : IEqualityComparer<PoiDTO>
    {

        public bool Equals(PoiDTO x, PoiDTO y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(PoiDTO obj)
        {
            return StaticGetHashCode(obj);
        }

        private static bool ArePoisEquivalent(PoiDTO x, PoiDTO y)
        {
            // Check if types are the same
            if (x.Type != y.Type) return false;
            if (ReferenceEquals(x.Body, y.Body)) return false;
            // but not Custom or None, or if their radii are equal
            return x.Type is not (PoiType.Custom or PoiType.None) || x.Radius.AreRelativelyEqual(y.Radius);
        }

        public static bool StaticEquals(PoiDTO x, PoiDTO y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            // Check if types are the same
            if (x.Type != y.Type) return false;

            // Check is bodies are the same
            if (!ReferenceEquals(x.Body, y.Body)) return false;

            // skip Radius check if Custom or None
            if (x.Type is not (PoiType.Custom or PoiType.None)) return true;

            // only thing we care about at this stage is if radius is different for Custom or None
            return x.Radius.AreRelativelyEqual(y.Radius);
        }

        public static int StaticGetHashCode(PoiDTO obj)
        {
            if (obj == null) return 0;

            var hash = 17; // Prime number to start with.
            // hash = hash * 31 + (obj.Label?.GetHashCode() ?? 0);
            // hash = hash * 31 + obj.Enabled.GetHashCode();
            if (obj.Type is not (PoiType.Custom or PoiType.None))
            {
                hash = hash * 31 + obj.Radius.GetHashCode();
            }
            // hash = hash * 31 + obj.Color.GetHashCode();
            hash = hash * 31 + obj.Type.GetHashCode();
            // CelestialBody will probably outlive the lifetime of the mod
            // ReSharper disable once Unity.NoNullPropagation
            hash = hash * 31 + (obj.Body?.GetHashCode() ?? 0);
            // hash = hash * 31 + obj.AddPlanetRadius.GetHashCode();
            // hash = hash * 31 + obj.LineWidth.GetHashCode();
            // hash = hash * 31 + obj.Resolution.GetHashCode();

            return hash;
        }
    }

}
