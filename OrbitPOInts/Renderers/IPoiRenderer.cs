using OrbitPOInts.Data.POI;

namespace OrbitPOInts
{
    public interface IPoiRenderer : IRenderer
    {
        public POI Poi { get; set; }
    }
}
