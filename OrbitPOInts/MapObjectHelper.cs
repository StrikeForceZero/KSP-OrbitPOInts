using System;
using UnityEngine;

namespace OrbitPOInts
{
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
