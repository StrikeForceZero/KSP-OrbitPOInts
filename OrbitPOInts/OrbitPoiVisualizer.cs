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

        public void SetEnabled(bool state)
        {
            DoActionOnSpheres(sphere => sphere.SetEnabled(state));
            DoActionOnCircles(circle => circle.SetEnabled(state));
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

        #region Refresh

        public void UpdateBody(CelestialBody body)
        {
            LogDebug($"[UpdateBody] body: {body.name}");
            if (!SafeToDraw("[UpdateBody]"))
            {
                return;
            }

            RefreshCurrentRenderers();
        }

        public void Refresh(MapObject focusTarget)
        {
            if (!SafeToDraw("[UpdateBody]"))
            {
                return;
            }

            if (focusTarget == null)
            {
                LogError("[Refresh] target is null!");
                return;
            }

            SetEnabled(true);

            var body = MapObjectHelper.GetTargetBody(focusTarget);
            LogDebug($"[Refresh] target: {MapObjectHelper.GetTargetName(focusTarget)}, body: {body.name}");
            UpdateBody(body);
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

        private void CreatePoisForBody(CelestialBody body, Action<POI> onCreatePoi)
        {
            foreach (PoiType poiType in Enum.GetValues(typeof(PoiType)))
            {
                if (poiType.IsNoneOrCustom()) continue;
                // check to make sure its not disabled in the global config
                if (!Settings.Instance.GetGlobalEnableFor(body, poiType))
                {
                    LogDebug($"[CreatePoisForBody] skipping - global disable for body:{body.Serialize()} type:{poiType}");
                    continue;
                }
                var poi = Settings.Instance.GetStandardPoiFor(body, poiType);

                // TODO: there is probably a better way to ensure the colors are respected in the correct order but this works for now
                var poiColor = Settings.Instance.GetPoiColorFor(body, poiType);
                // clone so we don't trigger a prop change event
                poi = poi.Clone();
                poi.Color = poiColor;

                switch (poiType)
                {
                    case PoiType.MaxTerrainAltitude when body.atmosphere && !Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies:
                    case PoiType.Atmosphere when !body.atmosphere:
                        LogDebug($"[CreatePoisForBody] skipping body:{body.Serialize()} type:{poiType} - atmosphere:{body.atmosphere} ShowPoiMaxTerrainAltitudeOnAtmosphericBodies:{Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies}");
                        continue;
                    default:
                        LogDebug($"[CreatePoisForBody] body:{body.Serialize()} type:{poi.Type} renderRadius:{poi.RadiusForRendering()} color:{poi.Color.Serialize()}");
                        onCreatePoi.Invoke(poi);
                        break;
                }
            }

            var globalCustomPois = Settings.Instance
                .GetCustomPoisFor(null)
                .Select(poi => poi.CloneWith(body)); // populate them with a body

            var customPois = Settings.Instance
                .GetCustomPoisFor(body)
                .Concat(globalCustomPois) // include global custom
                .Where(poi => poi.Enabled && poi.RadiusForRendering() > 0); // only ones that are enabled and have a radius
            foreach (var customPoi in customPois)
            {
                LogDebug($"[CreatePoisForBody] body:{customPoi.Body.Serialize()} type:{customPoi.Type} renderRadius:{customPoi.RadiusForRendering()} color:{customPoi.Color.Serialize()}");
                onCreatePoi.Invoke(customPoi);
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
            CreatePoisForBody(body, poi =>
            {
                if (mode is CreateMode.All or CreateMode.Circles)
                {
                    LogDebug($"[CreateBodyItems]: Generating circle around {body.name} {poi.Type} {poi.RadiusForRendering()}");
                    CreateCircleFromPoi(poi);
                }

                if (mode is CreateMode.All or CreateMode.Spheres)
                {
                    LogDebug($"[CreateBodyItems]: Generating sphere around {body.name} {poi.Type} {poi.RadiusForRendering()}");
                    CreateWireSphereFromPoi(poi);
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
            SetEnabledCircles(DrawCircles);
            SetEnabledSpheres(DrawSpheres);
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

        private PoiRenderReference CreateWireSphereFromPoi(POI poi)
        {
            LogDebug($"[CreateWireSphereFromPoi]: Generating spheres around body: {poi.Body.Serialize()}, color:{poi.Color.Serialize()}, radius:{poi.RadiusForRendering()}, line:{poi.LineWidth}, res: {poi.Resolution}");
            var poiRenderReference = _poiRenderReferenceManager.GetOrCreatePoiRenderReference(poi);
            var sphereRenderReference = poiRenderReference.CreateAndReplaceSphere();
            SetWireSphere(sphereRenderReference, poi.Body, poi.Color, (float)poi.RadiusForRendering(), poi.LineWidth, poi.Resolution);
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

            sphere.SetEnabled(DrawSpheres);
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

        private PoiRenderReference CreateCircleFromPoi(POI poi)
        {
            LogDebug($"[CreateCircleFromPoi]: Generating circle around body: {poi.Body.Serialize()}, color:{poi.Color.Serialize()}, radius:{poi.RadiusForRendering()}, line:{poi.LineWidth}, res: {poi.Resolution}");
            var poiRenderReference = _poiRenderReferenceManager.GetOrCreatePoiRenderReference(poi);
            var circleRenderReference = poiRenderReference.CreateAndReplaceCircle();
            SetCircle(circleRenderReference, poi.Body, poi.Color, (float)poi.RadiusForRendering(), poi.LineWidth);
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

            circle.SetEnabled(DrawCircles);
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
