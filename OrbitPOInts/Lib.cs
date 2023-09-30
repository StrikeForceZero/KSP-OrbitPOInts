using System;
using System.Collections;

#if TEST
using UnityEngineMock;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_Vector3d = KSPMock.Vector3d;
using KSP_Vessel = KSPMock.Vessel;
using KSP_GameScenes = KSPMock.GameScenes;
using KSP_HighLogic = KSPMock.HighLogic;
using KSP_MapView = KSPMock.MapView;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_CelestialBody = CelestialBody;
using KSP_Vector3d = Vector3d;
using KSP_Vessel = Vessel;
using KSP_GameScenes = GameScenes;
using KSP_HighLogic = HighLogic;
using KSP_MapView = MapView;
#endif


namespace OrbitPOInts
{
    using Vessel = KSP_Vessel;
    using Vector3d = KSP_Vector3d;
    using CelestialBody = KSP_CelestialBody;
    using GameScenes = KSP_GameScenes;
    using HighLogic = KSP_HighLogic;
    using MapView = KSP_MapView;

    public static class Lib
    {
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
    }
}
