using System;

namespace OrbitPOInts.Data
{
    sealed class CustomPOI
    {
        public Func<bool> Enabled { get; set; }
        public Func<double> Diameter { get; set; }
        public PoiType PoiType { get; set; }
    }
}
