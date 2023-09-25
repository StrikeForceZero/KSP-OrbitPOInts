using System;
using System.Collections;
using UnityEngine;

namespace OrbitPOInts
{
    public static class Lib
    {
        public static double GetApproxTerrainMaxHeight(CelestialBody body, int sampleResolution = 100)
        {
            var maxAltitude = Double.NegativeInfinity;

            for (var i = 0; i <= sampleResolution; i++)
            {
                for (var j = 0; j <= sampleResolution; j++)
                {
                    var latitude = (i / (double)sampleResolution) * 180 - 90;
                    var longitude = (j / (double)sampleResolution) * 360 - 180;

                    var altitude = body.TerrainAltitude(latitude, longitude, true);
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
            var targetRotation = Quaternion.FromToRotation(Vector3.up, normal);
            if (transform.rotation == targetRotation) return;
            // Logger.Log($"[AlignTransformToNormal] {transform.rotation} -> {normal}");
            transform.rotation = targetRotation;
        }

        // TODO: MapView.MapIsEnabled and HighLogic.LoadedScene == GameScenes.FLIGHT on quicksave load
        public static bool ViewingMapOrTrackingStation =>
            MapView.MapIsEnabled || HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public static string GetSceneName(GameScenes scene)
        {
            return Enum.GetName(typeof(GameScenes), scene);
        }

        public delegate void ActionToDelay();
        public static IEnumerator DelayedAction(ActionToDelay action, uint framesToDelay = 1)
        {
            for (var i = 0; i < framesToDelay; i++)
            {
                yield return null; // Wait for the next frame
            }

            action.Invoke();
        }
    }
}
