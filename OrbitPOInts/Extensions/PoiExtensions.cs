using OrbitPOInts.Data.POI;

namespace OrbitPOInts.Extensions
{
    public static class PoiExtensions
    {
        public static bool IsGlobal(this POI poi)
        {
            return poi.Body == null;
        }
    }
}
