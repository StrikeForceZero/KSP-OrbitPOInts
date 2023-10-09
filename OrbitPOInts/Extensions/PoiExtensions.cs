using OrbitPOInts.Data.POI;

namespace OrbitPOInts.Extensions
{
    public static class PoiExtensions
    {
        public static bool IsGlobal(this POI poi)
        {
            return poi.Body == null;
        }

        public static string GetDerivedClassName(this POI poi)
        {
            return poi switch
            {
                ResettablePoi => nameof(ResettablePoi),
                CustomPOI => nameof(CustomPOI),
                _ => nameof(POI)
            };
        }
    }
}
