using System;
using UnityEngine;

namespace OrbitPOInts
{
    public static class Lib
    {
        public static double GetApproxTerrainMaxHeight(CelestialBody body, int sampleResolution = 100)
        {
            double maxAltitude = Double.NegativeInfinity;

            for (int i = 0; i <= sampleResolution; i++)
            {
                for (int j = 0; j <= sampleResolution; j++)
                {
                    double latitude = (i / (double)sampleResolution) * 180 - 90;
                    double longitude = (j / (double)sampleResolution) * 360 - 180;

                    double altitude = body.TerrainAltitude(latitude, longitude, true);
                    maxAltitude = Math.Max(maxAltitude, altitude);
                }
            }

            return maxAltitude;
        }

        public static Vector3 GetCorrectedOrbitalNormal(Vessel vessel)
        {
            return TransformOrbitToWorld(vessel.orbit.GetOrbitNormal());
        }

        // GetOrbitNormal() - A unit vector normal to the plane of the orbit. NOTE: All Vector3d's returned by Orbit class functions have their y and z axes flipped. You have to flip these back to get the vectors in world coordinates.
        public static Vector3d TransformOrbitToWorld(Vector3d orbitVector)
        {
            return new Vector3d(orbitVector.x, orbitVector.z, orbitVector.y);
        }

        public static void AlignTransformToNormal(Transform transform, Vector3d normal)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, normal);
            transform.rotation = targetRotation;
        }
    }
}