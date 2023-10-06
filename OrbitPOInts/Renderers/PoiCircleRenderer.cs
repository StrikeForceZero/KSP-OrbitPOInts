using OrbitPOInts.Data.POI;

namespace OrbitPOInts
{
    public class PoiCircleRenderer : CircleRenderer, IPoiRenderer
    {
        public POI Poi { get; set; }
    }
}
