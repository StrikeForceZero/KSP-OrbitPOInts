using System;

namespace OrbitPOInts.Data
{
    // TODO: not sure theres much benefit for having a separate CustomPOI class
    sealed class CustomPOI : POI
    {
        public CustomPOI(CelestialBody body = null) : base(PoiType.Custom, body)
        {

        }
    }
}
