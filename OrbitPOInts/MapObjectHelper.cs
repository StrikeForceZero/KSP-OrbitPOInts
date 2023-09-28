using System;

#if TEST
using UnityEngineMock;
using System.Linq;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_MapObject = KSPMock.MapObject;
using KSP_Vessel = KSPMock.Vessel;
#else
using UniLinq;
using UnityEngine;
using KSP_CelestialBody = CelestialBody;
using KSP_MapObject = MapObject;
using KSP_Vessel = Vessel;
#endif

namespace OrbitPOInts
{
    using CelestialBody = KSP_CelestialBody;
    using MapObject = KSP_MapObject;
    using Vessel = KSP_Vessel;

    public static class MapObjectHelper
    {
        public static string GetTargetName(MapObject target)
        {
            if (target.vessel != null)
            {
                return target.name;
            }

            if (target.celestialBody)
            {
                return target.celestialBody.name;
            }

            return "no target!";
        }

        public static CelestialBody GetTargetBody(MapObject target)
        {
            if (target.vessel != null)
            {
                return target.vessel.mainBody;
            }

            if (target.celestialBody)
            {
                return target.celestialBody;
            }

            throw new Exception("bad target, no celestial body");
        }

        public static string GetVesselName(Vessel vessel)
        {
            if (vessel == null)
            {
                return "no vessel";
            }

            return vessel.name;
        }

        public static string GetBodyName(CelestialBody body)
        {
            return body == null ? "no body" : body.name;
        }

        public static Vector3 GetVesselOrbitNormal(Vessel vessel)
        {
            return vessel != null ? Lib.GetCorrectedOrbitalNormal(vessel) : Vector3.zero;
        }
    }
}
