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
            LogDebug($"[RemovePoi] {Logger.GetPoiLogId(poi)}");
            if (poi.Body == null)
            {
                foreach (var body in FlightGlobals.Bodies)
                {
                    poi = poi.CloneWith(body, true);
                    _poiRenderReferenceManager.RemovePoiRenderReference(poi);
                }
                return;
            }
            _poiRenderReferenceManager.RemovePoiRenderReference(poi);
        }

        public void AddPoi(POI poi)
        {
            LogDebug($"[AddPoi] {Logger.GetPoiLogId(poi)}");
            if (poi.Body == null)
            {
                foreach (var body in FlightGlobals.Bodies)
                {
                    poi = poi.CloneWith(body, true);
                    CreateNewPoiRender(poi, (poi, enabled) =>
                    {
                        CreateCircleFromPoi(poi, enabled);
                        CreateWireSphereFromPoi(poi, enabled);
                    });
                }
                return;
            }
            CreateNewPoiRender(poi, (poi, enabled) =>
            {
                CreateCircleFromPoi(poi, enabled);
                CreateWireSphereFromPoi(poi, enabled);
            });
        }

        // TODO: this needs a unit test...
        public void ResetStandardPoi(POI maybeUnpatchedPoi)
        {
            LogDebug($"[ResetPoi] {Logger.GetPoiLogId(maybeUnpatchedPoi)}");
            // we need to patch in the body to accept global pois
            var patchedPois = maybeUnpatchedPoi.IsGlobal()
                // need to patch
                ? FlightGlobals.Bodies.Select(body =>
                {
                    LogDebug($"[ResetPoi] patching global poi {Logger.GetPoiLogId(maybeUnpatchedPoi)} with body {body.Serialize()}");
                    return maybeUnpatchedPoi.CloneWith(body, true);
                })
                // not global, no need to patch
                : new List<POI> { maybeUnpatchedPoi };
            foreach (var patchedPoi in patchedPois)
            {
                // redundant but just in case because tracking these state issues down gives you gray hairs
                if (patchedPoi.IsGlobal())
                {
                    throw new ApplicationException("encountered un-patched global poi!");
                }

                LogDebug($"[ResetPoi] {Logger.GetPoiLogId(patchedPoi)} getting renderers");
                // TODO: in GameStateManager we have GetRenderReferencesForPoi to check if there are any render references
                // maybe we just move the check to the get method with an optional boolean to log error?
                foreach (var (poiRenderReference, renderer) in PoiRenderReferenceManager.GetAllRenderReferencesRendererTuplesForPoi(patchedPoi))
                {
                    var poi = patchedPoi;
                    if (patchedPoi.Type.IsStandard())
                    {
                        poi = Settings.Instance.GetConfiguredOrDefaultPoiFor(patchedPoi.Body, patchedPoi.Type);
                        // if we get a global again then just reset it back to patchedPoi
                        // TODO: this is really ugly, need to investigate
                        if (poi.IsGlobal())
                        {
                            poi = patchedPoi;
                        }
                    }

                    LogDebug($"[ResetPoi] updating PoiRenderReference.Poi with {Logger.GetPoiLogId(poi)}");
                    poiRenderReference.UpdatePoi(poi);
                    LogDebug($"[ResetPoi] resetting LineWidth for {Logger.GetPoiLogId(poi)}");
                    renderer.SetWidth(ScaleLineWidth(poi.RadiusForRendering(), poi.LineWidth));
                    LogDebug($"[ResetPoi] resetting Color for {Logger.GetPoiLogId(poi)}");
                    renderer.SetColor(poi.Color);
                }
            }
        }

        public void SetEnabled(bool state)
        {
            enabled = state;
            RefreshCurrentRenderers();
        }

        private bool SafeToDraw(string tag = "")
        {
            if (!enabled)
            {
                LogError($"[SafeToDraw]{tag} called when not enabled!");
                return false;
            }
            if (!SceneHelper.ViewingMapOrTrackingStation)
            {
                LogError($"[SafeToDraw]{tag} called not ViewingMapOrTrackingStation!");
                return false;
            }
            if (Context.GameState.IsSceneLoading)
            {
                LogError($"[SafeToDraw]{tag} called when scene loading!");
                return false;
            }

            return true;
        }

        public void Init()
        {
            LogDebug("[Init]");
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
            List<IRenderer> renderersThatNeedTransformsAligned = new();

            DoActionOnCircles(circle => renderersThatNeedTransformsAligned.Add(circle));

            if (AlignSpheres)
            {
                DoActionOnSpheres(sphere => renderersThatNeedTransformsAligned.Add(sphere));
            }
            else
            {
                List<IRenderer> rendererThatNeedTransformsReset = new();
                DoActionOnSpheres(sphere => rendererThatNeedTransformsReset.Add(sphere));
                foreach (var renderer in rendererThatNeedTransformsReset)
                {
                    // if we dont set transform.rotation = Quaternion.identity directly some alignments will be off
                    Context.StartCoroutine(DelayedAction.CreateCoroutine(() =>
                    {
                        // we are calling from the previous frame
                        // since things could have been destroyed and that's ok
                        // also we cant set the rotation if its not enabled otherwise it will end up bugged/flipped
                        if (renderer is not { enabled: true } || renderer.GetTransform() == null) return;
                        // reset rotation
                        renderer.GetTransform().localRotation = Quaternion.identity;
                    }));
                }
            }

            foreach (var renderer in renderersThatNeedTransformsAligned)
            {
                NextFrameAlignRendererTransformToNormal(renderer, normal);
            }
        }

        private void NextFrameAlignRendererTransformToNormal(IRenderer renderer, Vector3d normal)
        {
            // this shouldn't be null, just a sanity check for other logging points
            if (renderer == null || renderer.GetTransform() == null)
            {
                Logger.LogError($"[NextFrameAlignRendererTransformToNormal] transform null!");
                return;
            }

            // we cant set the rotation if its not enabled otherwise it will end up bugged/flipped
            if (!renderer.enabled) return;

            Context.StartCoroutine(DelayedAction.CreateCoroutine(() =>
            {
                // we cant set the rotation if its not enabled otherwise it will end up bugged/flipped
                if (renderer is not { enabled: true } || renderer.GetTransform() == null) return;
                AlignTransformToNormal(renderer.GetTransform(), normal, true);
            }, 1));
        }

        private void NextFrameAlignTransformToNormal(Transform transform, Vector3d normal)
        {
            // this shouldn't be null, just a sanity check for other logging points
            if (transform == null)
            {
                Logger.LogError($"[NextFrameAlignTransformToNormal] transform null!");
                return;
            }

            Context.StartCoroutine(DelayedAction.CreateCoroutine(() => AlignTransformToNormal(transform, normal, true), 1));
        }

        private void AlignTransformToNormal(Transform transform, Vector3d normal, bool isCalledFromPreviousFrame = false)
        {
            if (!enabled)
            {
                return;
            }
            // sometimes the transform can be null if called from a previous frame
            if (transform == null)
            {
                // don't log if we are calling this from a previous frame
                // since things could have been destroyed and that's ok
                if (isCalledFromPreviousFrame) return;
                // but just in case, if we arent from the previous frame we need to know
                Logger.LogError($"[AlignTransformToNormal] transform null!");
                return;
            }
            OrbitHelpers.AlignTransformToNormal(transform, normal);
        }
        #endregion

        // TODO: this needs a test case
        private bool CalcPoiEnabled(POI poi)
        {
            if (poi.IsGlobal())
            {
                throw new InvalidOperationException("CalcPoiEnabled should not be used for globals");
            }
            // check to make sure its not disabled in the global config
            // ignore if its not a standard poi to prevent NRE
            var globallyDisabled = poi.Type.IsStandard() && !Settings.Instance.GetGlobalEnableFor(poi.Body, poi.Type);
            var poiEnabled = poi.Enabled;

            switch (poi.Type)
            {
                case PoiType.MaxTerrainAltitude when poi.Body.atmosphere && !Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies:
                case PoiType.Atmosphere when !poi.Body.atmosphere:
                    poiEnabled = false;
                    break;
                default:
                    break;
            }

            return !globallyDisabled && poiEnabled;
        }

        private void CreateNewPoiRender(POI poi, Action<POI, bool> onCreatePoi)
        {
            if (poi.Type.IsNone()) return;

            // only if standard poi - otherwise GetPoiColorFor will throw
            if (poi.Type.IsStandard())
            {
                var colorOverride = Settings.Instance.GetPoiColorFor(poi.Body, poi.Type);
                // this is only triggered if we are trying to draw the poi for a global
                // and the specific body poi color has been user configured
                if (poi.Color != colorOverride)
                {
                    // TODO: there is probably a better way to enforce color
                    // clone so we don't create property change events
                    poi = poi.Clone(true);
                    LogDebug($"[CreateNewPoiRender] overriding color for {Logger.GetPoiLogId(poi)} color: {poi.Color} -> {colorOverride}");
                }
                poi.Color = colorOverride;
            }

            onCreatePoi.Invoke(poi, CalcPoiEnabled(poi));
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
                .Select(poi => poi.CloneWith(body, true)); // populate them with a body

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
            foreach (var (poi, render) in _poiRenderReferenceManager.GetAllRenderPoiReferenceRenderersTuples<TRenderer>())
            {
                var originalEnabledState = render.enabled;
                var newEnabledState = state && CalcPoiEnabled(poi);
                if (FocusedBodyOnly)
                {
                    newEnabledState &= poi.Body == Context.GameState.FocusedOrActiveBody;
                }

                if (originalEnabledState != newEnabledState)
                {
                    LogDebug($"[SetEnabledRenderers] enabled state change for {Logger.GetPoiLogId(poi)} {originalEnabledState} -> {newEnabledState}");
                }
                render.SetEnabled(newEnabledState);
            }
        }

        public void RefreshCurrentRenderers()
        {
            LogDebug("[RefreshCurrentRenderers]");
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
            if (sphereRenderReference == null)
            {
                LogError("circleRenderReference null wtf");
            }
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
            sphere.lineWidth = ScaleLineWidth(radius, width);
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
            if (circleRenderReference == null)
            {
                LogError("circleRenderReference null wtf");
            }
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
            circle.lineWidth = ScaleLineWidth(radius, width);
            circle.segments = segments;

            circle.transform.SetParent(body.MapObject.trf);
            circle.transform.localRotation = Quaternion.identity;
            circle.transform.localScale = Vector3.one;
            circle.transform.localPosition = Vector3.zero;
        }

        #endregion

        #region MISC

        private static float ScaleLineWidth(double radius, float width)
        {
            return (float)(radius / _standardLineWidthDistance) * width;
        }

        private static double GetStandardLineWidthDistance()
        {
            return FlightGlobals.Bodies.Where(body => body.isHomeWorld).Select(body => body.minOrbitalDistance).FirstOrDefault();
        }
        #endregion
    }
}
