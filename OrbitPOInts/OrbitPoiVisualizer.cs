using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using OrbitPOInts.Data;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;
using OrbitPOInts.Extensions.Unity;
using OrbitPOInts.Helpers;
using OrbitPOInts.Utils;

#if TEST
using UnityEngineMock;
using System.Linq;
using KSP_KSPAddon = KSPMock.KSPAddon;
using KSP_HighLogic = KSPMock.HighLogic;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_Vector3d = KSPMock.Vector3d;
using KSP_Vessel = KSPMock.Vessel;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
using KSP_GameEvents = KSPMock.GameEvents;
using KSP_PlanetariumCamera = KSPMock.PlanetariumCamera;
using KSP_MapObject = KSPMock.MapObject;
using KSP_GameScenes = KSPMock.GameScenes;
using KSP_ScaledSpace = KSPMock.ScaledSpace;
#else
using UniLinq;
using UnityEngine;
using KSP_KSPAddon = KSPAddon;
using KSP_HighLogic = HighLogic;
using KSP_CelestialBody = CelestialBody;
using KSP_Vector3d = Vector3d;
using KSP_Vessel = Vessel;
using KSP_FlightGlobals = FlightGlobals;
using KSP_GameEvents = GameEvents;
using KSP_PlanetariumCamera = PlanetariumCamera;
using KSP_MapObject = MapObject;
using KSP_GameScenes = GameScenes;
using KSP_ScaledSpace = ScaledSpace;
#endif

namespace OrbitPOInts
{
    using KSPAddon = KSP_KSPAddon;
    using HighLogic = KSP_HighLogic;
    using Vessel = KSP_Vessel;
    using Vector3d = KSP_Vector3d;
    using CelestialBody = KSP_CelestialBody;
    using FlightGlobals = KSP_FlightGlobals;
    using GameEvents = KSP_GameEvents;
    using PlanetariumCamera = KSP_PlanetariumCamera;
    using MapObject = KSP_MapObject;
    using GameScenes = KSP_GameScenes;
    using ScaledSpace = KSP_ScaledSpace;

    using Logger = Utils.Logger;

    public class OrbitPoiVisualizer<TContext> where TContext : MonoBehaviour, IHasGameState
    {
        private readonly PoiRenderReferenceManager<TContext> _poiRenderReferenceManager;

        public bool DrawSpheres;
        public bool DrawCircles;
        public bool AlignSpheres;
        public bool FocusedBodyOnly;
        public bool enabled = true;

        private static double _standardLineWidthDistance;

        public OrbitPoiVisualizer(TContext context)
        {
            Context = context;
            _standardLineWidthDistance = GetStandardLineWidthDistance();
            _poiRenderReferenceManager = new PoiRenderReferenceManager<TContext>(context);
        }

        private readonly TContext Context;

        public PoiRenderReferenceManager<TContext> PoiRenderReferenceManager => _poiRenderReferenceManager;

        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[OrbitPoiVisualizer] {message}");
        }

        private static void Log(string message)
        {
            Logger.Log($"[OrbitPoiVisualizer] {message}");
        }

        private static void LogError(string message)
        {
            Logger.LogError($"[OrbitPoiVisualizer] {message}");
        }

        private void DoActionOnSpheres(Action<WireSphereRenderer> action)
        {
            foreach (var sphere in _poiRenderReferenceManager.GetAllRenderReferenceRenderers<WireSphereRenderer>())
            {
                if (!sphere.IsAlive() || sphere.IsDying)
                {
                    LogError($"[DoActionOnCircles] sphere null or dying");
                    continue;
                }
                action.Invoke(sphere);
            }
        }

        private void DoActionOnCircles(Action<CircleRenderer> action)
        {
            foreach (var circle in _poiRenderReferenceManager.GetAllRenderReferenceRenderers<CircleRenderer>())
            {
                if (!circle.IsAlive() || circle.IsDying)
                {
                    LogError($"[DoActionOnCircles] circle null or dying");
                    continue;
                }
                action.Invoke(circle);
            }
        }

        public void RemovePoi(POI poi)
        {
            _poiRenderReferenceManager.RemovePoiRenderReference(poi);
        }

        public void AddPoi(POI poi)
        {
            CreateNewPoiRender(poi, (poi, enabled) =>
            {
                CreateCircleFromPoi(poi, enabled);
                CreateWireSphereFromPoi(poi, enabled);
            });
        }

        public void SetEnabled(bool state)
        {
            RefreshCurrentRenderers();
            enabled = state;
        }

        private bool SafeToDraw(string tag = "")
        {
            if (!enabled)
            {
                LogError($"[SafeToDraw]{tag} UpdateBody called when not enabled!");
                return false;
            }
            if (!SceneHelper.ViewingMapOrTrackingStation)
            {
                LogError($"[SafeToDraw]{tag} UpdateBody called not ViewingMapOrTrackingStation!");
                return false;
            }
            if (Context.GameState.IsSceneLoading)
            {
                LogError($"[SafeToDraw]{tag} UpdateBody called when scene loading!");
                return false;
            }

            return true;
        }

        public void Init()
        {
            foreach (var body in FlightGlobals.Bodies)
            {
                CreateBodyItems(body);
            }
            RefreshCurrentRenderers();
        }

        #region Refresh
        public void Refresh(MapObject focusTarget)
        {
            if (!SafeToDraw("[Refresh]"))
            {
                return;
            }

            if (focusTarget == null)
            {
                LogError("[Refresh] target is null!");
                return;
            }

            var body = MapObjectHelper.GetTargetBody(focusTarget);
            LogDebug($"[Refresh] target: {MapObjectHelper.GetTargetName(focusTarget)}, body: {body.name}");
            RefreshCurrentRenderers();
        }

        public void CurrentTargetRefresh()
        {
            Refresh(PlanetariumCamera.fetch.target);
        }

        // TODO: this method was supposed to be for optimization and reusing existing objects
        // but for sanity sake we are just gonna purge everything until the state machine is rewritten
        internal void RemoveAll()
        {
            _poiRenderReferenceManager.Clear();
        }

        #endregion

        #region Normals
        public void UpdateNormals(Vector3 normal)
        {
            if (!enabled || Context.GameState.IsSceneLoading || !SceneHelper.ViewingMapOrTrackingStation)
            {
                return;
            }
            List<Transform> transformsNeedsUpdate = new();

            DoActionOnSpheres(sphere => transformsNeedsUpdate.Add(sphere.transform));

            if (AlignSpheres)
            {
                DoActionOnCircles(circle => transformsNeedsUpdate.Add(circle.transform));
            }

            foreach (var transform in transformsNeedsUpdate)
            {
                NextFrameAlignTransformToNormal(transform, normal);
            }
        }

        private void NextFrameAlignTransformToNormal(Transform transform, Vector3d normal)
        {
            if (transform == null)
            {
                Logger.LogError($"[NextFrameAlignTransformToNormal] transform null!");
                return;
            }

            Context.StartCoroutine(DelayedAction.CreateCoroutine(() => AlignTransformToNormal(transform, normal), 1));
        }

        private void AlignTransformToNormal(Transform transform, Vector3d normal)
        {
            if (!enabled)
            {
                return;
            }
            // TODO: sometimes the transform can be null
            if (transform == null)
            {
                Logger.LogError($"[AlignTransformToNormal] transform null!");
                return;
            }
            OrbitHelpers.AlignTransformToNormal(transform, normal);
        }
        #endregion

        private void CreateNewPoiRender(POI poi, Action<POI, bool> onCreatePoi)
        {
            if (poi.Type.IsNone()) return;

            // check to make sure its not disabled in the global config
            var globallyDisabled = !Settings.Instance.GetGlobalEnableFor(poi.Body, poi.Type);
            var poiEnabled = poi.Enabled;

            if (poi.Type.IsStandard())
            {
                poi.Color = Settings.Instance.GetPoiColorFor(poi.Body, poi.Type);
            }

            switch (poi.Type)
            {
                case PoiType.MaxTerrainAltitude when poi.Body.atmosphere && !Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies:
                case PoiType.Atmosphere when !poi.Body.atmosphere:
                    poiEnabled = false;
                    break;
                default:
                    break;
            }

            onCreatePoi.Invoke(poi, !globallyDisabled && poiEnabled);
        }

        private void CreatePoisForBody(CelestialBody body, Action<POI, bool> onCreatePoi)
        {
            foreach (PoiType poiType in Enum.GetValues(typeof(PoiType)))
            {
                if (poiType.IsNoneOrCustom()) continue;
                var poi = Settings.Instance.GetStandardPoiFor(body, poiType);
                CreateNewPoiRender(poi, onCreatePoi);
            }

            var globalCustomPois = Settings.Instance
                .GetCustomPoisFor(null)
                .Select(poi => poi.CloneWith(body)); // populate them with a body

            var customPois = Settings.Instance
                .GetCustomPoisFor(body)
                .Concat(globalCustomPois); // include global custom
            foreach (var customPoi in customPois)
            {
                onCreatePoi.Invoke(customPoi, customPoi.Enabled);
            }
        }

        private enum CreateMode
        {
            Spheres,
            Circles,
            All,
        }
        private void CreateBodyItems(CelestialBody body, CreateMode mode = CreateMode.All)
        {
            CreatePoisForBody(body, (poi, enabled) =>
            {
                if (mode is CreateMode.All or CreateMode.Circles)
                {
                    LogDebug($"[CreateBodyItems]: Generating circle around {body.name} {poi.Type} {poi.RadiusForRendering()}");
                    CreateCircleFromPoi(poi, enabled);
                }

                if (mode is CreateMode.All or CreateMode.Spheres)
                {
                    LogDebug($"[CreateBodyItems]: Generating sphere around {body.name} {poi.Type} {poi.RadiusForRendering()}");
                    CreateWireSphereFromPoi(poi, enabled);
                }
            });
        }

        public void SetEnabledRenderers<TRenderer>(bool state) where TRenderer : MonoBehaviour, IRenderer
        {
            foreach ((var poi, var render) in _poiRenderReferenceManager.GetAllRenderPoiReferenceRenderersTuples<TRenderer>())
            {
                if (FocusedBodyOnly)
                {
                    render.SetEnabled(poi.Enabled && state && poi.Body == Context.GameState.FocusedOrActiveBody);
                }
                else
                {
                    render.SetEnabled(poi.Enabled && state);
                }
            }
        }

        public void RefreshCurrentRenderers()
        {
            var safeToDraw = SafeToDraw("[RefreshCurrentRenderers]");
            SetEnabledCircles(safeToDraw && DrawCircles);
            SetEnabledSpheres(safeToDraw && DrawSpheres);
        }

        #region Spheres

        public void SetEnabledSpheres(bool state)
        {
            SetEnabledRenderers<WireSphereRenderer>(state);
        }

        public void DestroyAndRecreateBodySpheres(CelestialBody targetObject)
        {
            RemoveBodySpheres();
            CreateBodySphere(targetObject);
        }

        private void RemoveBodySpheres()
        {
            foreach (var poiRenderReference in _poiRenderReferenceManager.AllPoiRenderReferences)
            {
                poiRenderReference.DestroySphereReference();
            }
        }

        private void CreateBodySphere(CelestialBody body)
        {
            CreateBodyItems(body, CreateMode.Spheres);
        }

        private PoiRenderReference CreateWireSphereFromPoi(POI poi, bool enabled = true)
        {
            LogDebug($"[CreateWireSphereFromPoi]: Generating spheres around body: {poi.Body.Serialize()}, color:{poi.Color.Serialize()}, radius:{poi.RadiusForRendering()}, line:{poi.LineWidth}, res: {poi.Resolution}");
            var poiRenderReference = _poiRenderReferenceManager.GetOrCreatePoiRenderReference(poi);
            var sphereRenderReference = poiRenderReference.CreateAndReplaceSphere();
            SetWireSphere(sphereRenderReference, poi.Body, poi.Color, (float)poi.RadiusForRendering(), poi.LineWidth, poi.Resolution);
            sphereRenderReference.SetEnabled(enabled);
            return poiRenderReference;
        }

        private void SetWireSphere(
            WireSphereRenderer sphere,
            CelestialBody body,
            Color color,
            float radius,
            float width = 1f,
            int resolution = 50
        )
        {

            sphere.wireframeColor = color;
            sphere.radius = radius * ScaledSpace.InverseScaleFactor;
            sphere.lineWidth = width;
            sphere.lineWidth = (float)(radius / _standardLineWidthDistance) * width;
            sphere.latitudeLines = resolution;
            sphere.longitudeLines = resolution;

            sphere.transform.SetParent(body.MapObject.trf);
            sphere.transform.localRotation = Quaternion.identity;
            sphere.transform.localScale = Vector3.one;
            sphere.transform.localPosition = Vector3.zero;
        }

        #endregion

        #region Circles

        public void SetEnabledCircles(bool state)
        {
            SetEnabledRenderers<CircleRenderer>(state);
        }

        public void DestroyAndRecreateBodyCircles(CelestialBody targetObject)
        {
            RemoveBodyCircles();
            CreateBodyCircle(targetObject);
        }

        private void RemoveBodyCircles()
        {
            foreach (var poiRenderReference in _poiRenderReferenceManager.AllPoiRenderReferences)
            {
                poiRenderReference.DestroyCircleReference();
            }
        }

        private void CreateBodyCircle(CelestialBody body)
        {
            CreateBodyItems(body, CreateMode.Circles);
        }

        private PoiRenderReference CreateCircleFromPoi(POI poi, bool enabled = true)
        {
            LogDebug($"[CreateCircleFromPoi]: Generating circle around body: {poi.Body.Serialize()}, color:{poi.Color.Serialize()}, radius:{poi.RadiusForRendering()}, line:{poi.LineWidth}, res: {poi.Resolution}");
            var poiRenderReference = _poiRenderReferenceManager.GetOrCreatePoiRenderReference(poi);
            var circleRenderReference = poiRenderReference.CreateAndReplaceCircle();
            SetCircle(circleRenderReference, poi.Body, poi.Color, (float)poi.RadiusForRendering(), poi.LineWidth);
            circleRenderReference.SetEnabled(enabled);
            return poiRenderReference;
        }

        private void SetCircle(
            CircleRenderer circle,
            CelestialBody body,
            Color color,
            float radius,
            float width = 1f,
            int segments = 360)
        {
            circle.wireframeColor = color;
            circle.radius = radius * ScaledSpace.InverseScaleFactor;
            circle.lineWidth = (float)(radius / _standardLineWidthDistance) * width;
            circle.segments = segments;

            circle.transform.SetParent(body.MapObject.trf);
            circle.transform.localRotation = Quaternion.identity;
            circle.transform.localScale = Vector3.one;
            circle.transform.localPosition = Vector3.zero;
        }

        #endregion

        #region MISC
        private static double GetStandardLineWidthDistance()
        {
            return FlightGlobals.Bodies.Where(body => body.isHomeWorld).Select(body => body.minOrbitalDistance).FirstOrDefault();
        }
        #endregion
    }
}
