using OrbitPOInts.Data.POI;

namespace OrbitPOInts
{
    public class PoiWireSphereRenderer : WireSphereRenderer, IPoiRenderer
    {
        public POI Poi { get; set; }
    }
}
