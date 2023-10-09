using OrbitPOInts.Data.POI;

namespace OrbitPOInts.Extensions
{
    public static class PoiTypeExtensions
    {
        public static bool IsStandard(this PoiType poiType)
        {
            return !poiType.IsNoneOrCustom();
        }

        public static bool IsNoneOrCustom(this PoiType poiType)
        {
            return poiType.IsNone() || poiType.IsCustom();
        }

        public static bool IsNone(this PoiType poiType)
        {
            return poiType == PoiType.None;
        }

        public static bool IsCustom(this PoiType poiType)
        {
            return poiType == PoiType.Custom;
        }
    }
}
