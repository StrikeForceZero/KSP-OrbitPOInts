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
    }
}
