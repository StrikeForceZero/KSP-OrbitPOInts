using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OrbitPOInts
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class OrbitPoiVisualizer : MonoBehaviour
    {
        public static OrbitPoiVisualizer Instance { get; private set; }

        private readonly List<WireSphereRenderer> _drawnSpheres = new();
        private readonly List<CircleRenderer> _drawnCircles = new();

        private bool _drawSpheres = true;
        private bool _drawCircles = true;
        private bool _rotateSpheres = false;

        private Vessel _lastVessel;
        private CelestialBody _lastOrbitingBody;

        private static double _standardLineWidthDistance;

        private static void LoadStandardLineWidthDistance()
        {
            foreach (var body in FlightGlobals.Bodies.Where(body => body.isHomeWorld))
            {
                _standardLineWidthDistance = body.minOrbitalDistance;
                break;
            }
        }

        #region EventFunctions
        private void Awake()
        {
            LoadStandardLineWidthDistance();
            Instance = this;
            GameEvents.OnMapEntered.Add(OnMapEntered);
            GameEvents.OnMapExited.Add(OnMapExited);
            GameEvents.OnMapFocusChange.Add(OnMapFocusChange);
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselSOIChanged.Add(OnVesselSOIChange);
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
        }

        private void OnDestroy()
        {
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.OnMapExited.Remove(OnMapExited);
            GameEvents.OnMapFocusChange.Remove(OnMapFocusChange);
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChange);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }
        
        private void Update()
        {
            if (!MapView.MapIsEnabled && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
            {
                // Utils.Log("[STATUS] not in map");
                return;
            }

            var target = PlanetariumCamera.fetch.target;
            // Utils.Log($"[UPDATE] vessel: {GetVesselName(target.vessel)}, body:{GetBodyName(target.celestialBody)}");
            var vessel = target.vessel;
            var hasVessel = vessel != null;
            var body = hasVessel ? vessel.mainBody : target.celestialBody;
            // Utils.Log($"[UPDATE] currentTarget: {(vessel ? vessel.name : body ? body.name : "no target")}");

            if (_lastVessel != vessel || _lastOrbitingBody != body)
            {
                Logger.Log($"[UPDATE] lastVessel: {MapObjectHelper.GetVesselName(_lastVessel)} -> {MapObjectHelper.GetVesselName(vessel)}, lastOrbitingBody: {MapObjectHelper.GetBodyName(_lastOrbitingBody)} -> {MapObjectHelper.GetBodyName(body)}");
                Refresh(target);

                _lastVessel = vessel;
                _lastOrbitingBody = body;
            }

            UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(vessel));
        }
        #endregion

        #region EventHandlers
        private void OnGameSceneLoadRequested(GameScenes scenes)
        {
            Logger.Log($"[MapOverlay] Load Requested");
            RemoveBodySpheres();
            RemoveBodyCircles();
        }

        private void OnMapEntered()
        {
            var target = PlanetariumCamera.fetch.target;
            Logger.Log($"[MapOverlay] Entering map view, focus on {MapObjectHelper.GetTargetName(target)}");

            Refresh(target);
        }

        private void OnMapExited()
        {
            Logger.Log($"[MapOverlay] Exiting map view");

            RemoveBodySpheres();
            RemoveBodyCircles();
        }

        private void OnVesselSOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> data)
        {
            Logger.Log($"[OnVesselSOIChainge] soi changed: {data.from.name} -> {data.to.name}");
            UpdateBody(data.to);
        }

        private void OnVesselChange(Vessel vessel)
        {
            Logger.Log($"[OnVesselChange] vessel changed: {MapObjectHelper.GetVesselName(_lastVessel)} -> {MapObjectHelper.GetVesselName(vessel)}");
            if (vessel == null)
            {
                return;
            }

            UpdateBody(vessel.mainBody);
            UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(vessel));
        }

        private void OnMapFocusChange(MapObject focusTarget)
        {
            Logger.Log($"[MapOverlay] Changed focus to {focusTarget.name}");
            Refresh(focusTarget);
        }        
        #endregion

        #region Refresh
        public void UpdateBody(CelestialBody body)
        {
            Logger.Log($"[UpdateBody] body: {body.name}");
            DestroyAndRecreateBodySpheres(body);
            DestroyAndRecreateBodyCircles(body);
        }
        
        private IEnumerator DelayedRefresh(MapObject target)
        {
            yield return new WaitForSeconds(0f);
            SyncRefresh(target);
        }

        private void SyncRefresh(MapObject target)
        {
            if (target == null)
            {
                Logger.Log("[Refresh] target is null!");
                return;
            }

            var body = MapObjectHelper.GetTargetBody(target);
            Logger.Log($"[Refresh] target: {MapObjectHelper.GetTargetName(target)}, body: {body.name}");
            UpdateBody(body);
        }

        public void Refresh(MapObject focusTarget)
        {
            // TODO: no idea why this is needed, but without calling it sync, then async, we can't align our circles to the orbit normal
            SyncRefresh(focusTarget);
            // TODO: better hook?
            StartCoroutine(DelayedRefresh(focusTarget));
        }
        #endregion

        private void UpdateNormals(Vector3 normal)
        {
            foreach (var circle in _drawnCircles)
            {
                Lib.AlignTransformToNormal(circle.transform, normal);
            }

            if (_rotateSpheres)
            {
                foreach (var sphere in _drawnSpheres)
                {
                    Lib.AlignTransformToNormal(sphere.transform, normal);
                }
            }
        }

        public void SetEnabled(bool state)
        {
            foreach (var p in _drawnSpheres)
            {
                p.SetEnabled(state);
            }

            foreach (var p in _drawnCircles)
            {
                p.SetEnabled(state);
            }
        }

        #region Spheres
        private void DestroyAndRecreateBodySpheres(CelestialBody targetObject)
        {
            RemoveBodySpheres();
            CreateBodySphere(targetObject);
        }

        private void RemoveBodySpheres()
        {
            Logger.Log($"[MapOverlay]: Removing body spheres");
            foreach (var sphere in _drawnSpheres)
            {
                Destroy(sphere);
            }

            _drawnSpheres.Clear();
        }

        private void CreateBodySphere(CelestialBody body)
        {
            if (!_drawSpheres)
            {
                return;
            }

            Logger.Log($"[MapOverlay]: Generating spheres around {body.name}");
            {
                CreateWireSphere(body, Color.white, (float)body.hillSphere, .1f, 50);
            }
            {
                // TODO: scale sampleRes based on body.Radius
                var maxAlt = body.Radius + Lib.GetApproxTerrainMaxHeight(body);
                CreateWireSphere(body, Color.red, (float)maxAlt, .01f, 55);
                // Utils.Log($"[MapOverlay]: Generated sphere maxAlt: {maxAlt} for {body.name}");
            }
            {
                CreateWireSphere(body, Color.magenta, (float)body.sphereOfInfluence, .05f, 50);
                // Utils.Log($"[MapOverlay]: Generated sphere sphereOfInfluence: {body.sphereOfInfluence} for {body.name}");
            }
            {
                CreateWireSphere(body, Color.green, (float)body.minOrbitalDistance, 0.01f, 50);
                // Utils.Log($"[MapOverlay]: Generated sphere minOrbitalDistance: {body.minOrbitalDistance} for {body.name}");
            }
            if (body.atmosphere)
            {
                var atmoDist = body.atmosphereDepth + body.Radius;
                CreateWireSphere(body, Color.cyan, (float)atmoDist, 0.01f, 40);
                // Utils.Log($"[MapOverlay]: Generated sphere atmoDist: {atmoDist} for {body.name}");
            }
        }

        private WireSphereRenderer CreateWireSphere(
            CelestialBody body,
            Color color,
            float radius,
            float width = 1f,
            int resolution = 50
        )
        {
            var sphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var sphere = sphereObj.AddComponent<WireSphereRenderer>();
            sphere.wireframeColor = color;
            sphere.radius = radius * ScaledSpace.InverseScaleFactor;
            sphere.lineWidth = width;
            sphere.latitudeLines = resolution;
            sphere.longitudeLines = resolution;

            sphere.transform.SetParent(body.MapObject.trf);
            sphere.transform.localRotation = Quaternion.identity;
            sphere.transform.localScale = Vector3.one;
            sphere.transform.localPosition = Vector3.zero;

            sphere.SetEnabled(true);
            _drawnSpheres.Add(sphere);

            return sphere;
        }
        #endregion

        #region Circles
        private void DestroyAndRecreateBodyCircles(CelestialBody targetObject)
        {
            RemoveBodyCircles();
            CreateBodyCircle(targetObject);
        }

        private void RemoveBodyCircles()
        {
            Logger.Log($"[MapOverlay]: Removing body circle");
            foreach (var circle in _drawnCircles)
            {
                Destroy(circle);
            }

            _drawnCircles.Clear();
        }

        private void CreateBodyCircle(CelestialBody body)
        {
            if (!_drawCircles)
            {
                return;
            }

            Logger.Log($"[MapOverlay]: Generating circles around {body.name}");
            {
                CreateCircle(body, Color.white, (float)body.hillSphere, 1f);
                // Utils.Log($"[MapOverlay]: Generated circle hillSphere: {body.hillSphere} for {body.name}");
            }
            {
                // TODO: scale sampleRes based on body.Radius
                var maxAlt = body.Radius + Lib.GetApproxTerrainMaxHeight(body);
                CreateCircle(body, Color.red, (float)maxAlt, 1f);
                // Utils.Log($"[MapOverlay]: Generated circle maxAlt: {maxAlt} for {body.name}");
            }
            {
                CreateCircle(body, Color.magenta, (float)body.sphereOfInfluence, 1f);
                // Utils.Log($"[MapOverlay]: Generated circle sphereOfInfluence: {body.sphereOfInfluence} for {body.name}");
            }
            {
                CreateCircle(body, Color.green, (float)body.minOrbitalDistance, 1f);
                // Utils.Log($"[MapOverlay]: Generated circle minOrbitalDistance: {body.minOrbitalDistance} for {body.name}");
            }
            if (body.atmosphere)
            {
                var atmoDist = body.atmosphereDepth + body.Radius;
                CreateCircle(body, Color.cyan, (float)atmoDist, 1f);
                // Utils.Log($"[MapOverlay]: Generated circle atmoDist: {atmoDist} for {body.name}");
            }
        }

        private CircleRenderer CreateCircle(CelestialBody body, Color color, float radius, float width = 1f,
            int segments = 360)
        {
            var circleObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var circle = circleObj.AddComponent<CircleRenderer>();

            circle.wireframeColor = color;
            circle.radius = radius * ScaledSpace.InverseScaleFactor; // (float)body.atmosphereDepth + radius?;
            circle.lineWidth = (float)(radius / _standardLineWidthDistance) * width;
            circle.segments = segments;

            circle.transform.SetParent(body.MapObject.trf);
            circle.transform.localRotation = Quaternion.identity;
            circle.transform.localScale = Vector3.one;
            circle.transform.localPosition = Vector3.zero;

            circle.SetEnabled(true);
            _drawnCircles.Add(circle);

            return circle;
        }
        #endregion
    }
}