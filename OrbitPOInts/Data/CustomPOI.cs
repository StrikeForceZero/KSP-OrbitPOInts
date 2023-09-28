#if TEST
using UnityEngineMock;
using KSP_ConfigNode = KSPMock.ConfigNode;
using KSP_CelestialBody = KSPMock.CelestialBody;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_ConfigNode = ConfigNode;
using KSP_CelestialBody = CelestialBody;
#endif

namespace OrbitPOInts.Data
{
    using CelestialBody = KSP_CelestialBody;
    using ConfigNode = KSP_ConfigNode;

    // TODO: not sure theres much benefit for having a separate CustomPOI class
    sealed class CustomPOI : POI
    {
        public CustomPOI(CelestialBody body = null) : base(PoiType.Custom, body)
        {

        }
    }
}
