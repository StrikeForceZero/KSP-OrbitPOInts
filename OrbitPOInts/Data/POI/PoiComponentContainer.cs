#if TEST
using KSPMock;
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.Data.POI
{
    public class PoiComponentContainer
    {
        public POI poi { get; set; }

        public GameObject SphereComponent { get; set; }
        public WireSphereRenderer SphereRenderer { get; set; }

        public GameObject CircleComponent { get; set; }
        public CircleRenderer CircleRenderer { get; set; }
    }
}
