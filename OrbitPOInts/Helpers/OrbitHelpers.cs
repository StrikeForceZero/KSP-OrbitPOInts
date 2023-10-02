#if TEST
using UnityEngineMock;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_Vector3d = KSPMock.Vector3d;
using KSP_Vessel = KSPMock.Vessel;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_CelestialBody = CelestialBody;
using KSP_Vector3d = Vector3d;
using KSP_Vessel = Vessel;
#endif

namespace OrbitPOInts.Helpers
{
    using Vessel = KSP_Vessel;
    using Vector3d = KSP_Vector3d;
    using CelestialBody = KSP_CelestialBody;

    public static class OrbitHelpers
    {
        public static Vector3 GetCorrectedOrbitalNormal(Vessel vessel)
        {
            return TransformOrbitToWorld(vessel.orbit.GetOrbitNormal());
        }

        // This exists because of this note for:
        // GetOrbitNormal() - A unit vector normal to the plane of the orbit.
        // NOTE: All Vector3d's returned by Orbit class functions have their y and z axes flipped.
        //      You have to flip these back to get the vectors in world coordinates.
        public static Vector3d TransformOrbitToWorld(Vector3d orbitVector)
        {
            return new Vector3d(orbitVector.x, orbitVector.z, orbitVector.y);
        }

        public static void AlignTransformToNormal(Transform transform, Vector3d normal)
        {
            var targetRotation = Quaternion.FromToRotation(Vector3.up, normal);
            if (transform.rotation == targetRotation) return;
            // Logger.Log($"[AlignTransformToNormal] {transform.rotation} -> {normal}");
            transform.rotation = targetRotation;
        }
    }
}
