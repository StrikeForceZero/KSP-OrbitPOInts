using System;

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

namespace OrbitPOInts.Helpers
{
    using GameScenes = KSP_GameScenes;
    using HighLogic = KSP_HighLogic;
    using MapView = KSP_MapView;

    public static class SceneHelper
    {
        // TODO: MapView.MapIsEnabled and HighLogic.LoadedScene == GameScenes.FLIGHT on quicksave load
        public static bool ViewingMapOrTrackingStation =>
            MapView.MapIsEnabled || HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public static string GetSceneName(GameScenes scene)
        {
            return Enum.GetName(typeof(GameScenes), scene);
        }
    }
}
