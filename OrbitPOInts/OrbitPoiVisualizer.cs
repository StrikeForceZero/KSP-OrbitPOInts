using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OrbitPOInts.Extensions;
using Smooth.Collections;
using UnityEngine;
using Enumerable = UniLinq.Enumerable;

namespace OrbitPOInts
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class OrbitPoiVisualizer : MonoBehaviour
    {
        public static OrbitPoiVisualizer Instance { get; private set; }

        private readonly Dictionary<string, GameObject> _bodyComponentHolders = new();
        private readonly HashSet<WireSphereRenderer> _drawnSpheres = new();
        private readonly HashSet<CircleRenderer> _drawnCircles = new();

        public bool DrawSpheres = true;
        public bool DrawCircles = true;
        public bool AlignSpheres = false;

        private Vessel _lastVessel;
        private CelestialBody _lastOrbitingBody;

        private static double _standardLineWidthDistance;
        
        private static CustomPOI[] _customPois = {
            new() { Enabled = () => Settings.CustomPOI1Enabled, Diameter = () => Settings.CustomPOI1 },
            new() { Enabled = () => Settings.CustomPOI2Enabled, Diameter = () => Settings.CustomPOI2 },
            new() { Enabled = () => Settings.CustomPOI3Enabled, Diameter = () => Settings.CustomPOI3 },
        };

        private static void LoadStandardLineWidthDistance()
        {
            foreach (var body in FlightGlobals.Bodies.Where(body => body.isHomeWorld))
            {
                _standardLineWidthDistance = body.minOrbitalDistance;
                break;
            }
        }

        private static void Log(string message)
        {
            Logger.Log($"[OrbitPoiVisualizer] {message}");
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
            foreach (var componentHolder in _bodyComponentHolders.Values)
            {
                DestroyImmediate(componentHolder);
            }
        }

        private void Update()
        {
            if (!Lib.ViewingMapOrTrackingStation)
            {
                return;
            }

            var target = PlanetariumCamera.fetch.target;
            var vessel = target.vessel;
            var hasVessel = vessel != null;
            var body = hasVessel ? vessel.mainBody : target.celestialBody;

            if (_lastVessel != vessel || _lastOrbitingBody != body)
            {
                Log(
                    $"[UPDATE] lastVessel: {MapObjectHelper.GetVesselName(_lastVessel)} -> {MapObjectHelper.GetVesselName(vessel)}, lastOrbitingBody: {MapObjectHelper.GetBodyName(_lastOrbitingBody)} -> {MapObjectHelper.GetBodyName(body)}");
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
            Log($"[MapOverlay] Load Requested");
            RemoveAll();
        }

        private void OnMapEntered()
        {
            var target = PlanetariumCamera.fetch.target;
            Log($"[MapOverlay] Entering map view, focus on {MapObjectHelper.GetTargetName(target)}");

            Refresh(target);
        }

        private void OnMapExited()
        {
            Log($"[MapOverlay] Exiting map view");
            PurgeAll();
        }

        private void OnVesselSOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> data)
        {
            Log($"[OnVesselSOIChange] soi changed: {data.from.name} -> {data.to.name}");
            UpdateBody(data.to);
        }

        private void OnVesselChange(Vessel vessel)
        {
            Log(
                $"[OnVesselChange] vessel changed: {MapObjectHelper.GetVesselName(_lastVessel)} -> {MapObjectHelper.GetVesselName(vessel)}");
            if (vessel == null || !Lib.ViewingMapOrTrackingStation)
            {
                return;
            }

            UpdateBody(vessel.mainBody);
            UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(vessel));
        }

        private void OnMapFocusChange(MapObject focusTarget)
        {
            Log($"[OnMapFocusChange] Changed focus to {focusTarget.name}");
            // TODO: this gets called when loading a save and we dont want to generate anything unless in map
            if (!Lib.ViewingMapOrTrackingStation)
            {
                Log("[OnMapFocusChange] Not in map, skipping event.");
                return;
            }
            Refresh(focusTarget);
        }

        #endregion

        #region Refresh

        public void UpdateBody(CelestialBody body)
        {
            Log($"[UpdateBody] body: {body.name}");
            PurgeAll();
            DestroyAndRecreateBodySpheres(body);
            DestroyAndRecreateBodyCircles(body);
        }

        public void Refresh(MapObject focusTarget)
        {
            if (focusTarget == null)
            {
                Log("[Refresh] target is null!");
                PurgeAll();
                return;
            }

            var body = MapObjectHelper.GetTargetBody(focusTarget);
            Log($"[Refresh] target: {MapObjectHelper.GetTargetName(focusTarget)}, body: {body.name}");
            UpdateBody(body);
        }

        public void CurrentTargetRefresh()
        {
            Refresh(PlanetariumCamera.fetch.target);
        }

        internal void RemoveAll()
        {
            RemoveBodySpheres();
            RemoveBodyCircles();
        }

        #endregion

        private void UpdateNormals(Vector3 normal)
        {
            List<Transform> transformsNeedsUpdate = new();
            foreach (var (circle, index) in _drawnCircles.Select((value, index) => (value, index)))
            {
                if (!circle.IsAlive() || circle.IsDying)
                {
                    Log($"[UpdateNormals] circle null or dying {index} / {_drawnCircles.Count}");
                    continue;
                }
                transformsNeedsUpdate.Add(circle.transform);
            }

            if (AlignSpheres)
            {
                foreach (var (sphere, index) in _drawnSpheres.Select((value, index) => (value, index)))
                {
                    if (!sphere.IsAlive() || sphere.IsDying)
                    {
                        Log($"[UpdateNormals] sphere null or dying {index} / {_drawnSpheres.Count}");
                        continue;
                    }
                    transformsNeedsUpdate.Add(sphere.transform);
                }
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
                Logger.Log($"[NextFrameAlignTransformToNormal] transform null!");
                return;
            }
            StartCoroutine(AsyncAlignTransformToNormal(transform, normal));
        }

        private IEnumerator AsyncAlignTransformToNormal(Transform transform, Vector3d normal)
        {
            yield return null;
            // TODO: sometimes the transform can be null
            if (transform == null)
            {
                Logger.Log($"[AsyncAlignTransformToNormal] transform null!");
                yield break;
            }
            Lib.AlignTransformToNormal(transform, normal);
        }

        public void SetEnabled(bool state)
        {
            foreach (var (sphere, index) in _drawnSpheres.Select((value, index) => (value, index)))
            {
                if (!sphere.IsAlive() || sphere.IsDying)
                {
                    Log($"[SetEnabled] sphere null or dying {index} / {_drawnSpheres.Count}");
                    continue;
                }
                sphere.SetEnabled(state);
            }

            foreach (var (circle, index) in _drawnCircles.Select((value, index) => (value, index)))
            {
                if (!circle.IsAlive() || circle.IsDying)
                {
                    Log($"[SetEnabled] circle null or dying {index} / {_drawnCircles.Count}");
                    continue;
                }
                circle.SetEnabled(state);
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
            PurgeSpheres();
        }

        private void CreateBodySphere(CelestialBody body)
        {
            if (!DrawSpheres)
            {
                return;
            }

            Log($"[MapOverlay]: Generating spheres around {body.name}");
            if (Settings.EnablePOI_HillSphere)
            {
                CreateWireSphere(body, Color.white, (float)body.hillSphere, .1f, 50);
            }

            var shouldShowMaxAlt =
                Settings.EnablePOI_MaxAlt && (!body.atmosphere || Settings.ShowPOI_MaxAlt_OnAtmoBodies);
            if (shouldShowMaxAlt)
            {
                // TODO: scale sampleRes based on body.Radius
                var maxAlt = body.Radius + Lib.GetApproxTerrainMaxHeight(body);
                CreateWireSphere(body, Color.red, (float)maxAlt, .01f, 55);
            }

            if (Settings.EnablePOI_SOI)
            {
                CreateWireSphere(body, Color.magenta, (float)body.sphereOfInfluence, .05f, 50);
            }

            if (Settings.EnablePOI_MinOrbit)
            {
                CreateWireSphere(body, Color.green, (float)body.minOrbitalDistance, 0.01f, 50);
            }

            if (body.atmosphere && Settings.EnablePOI_Atmo)
            {
                var atmoDist = body.atmosphereDepth + body.Radius;
                CreateWireSphere(body, Color.cyan, (float)atmoDist, 0.01f, 40);
            }
            
            foreach (var customPoi in Enumerable.Where(_customPois, poi => poi.Enabled() && poi.Diameter() > 0))
            {
                // TODO: custom color and specific body
                CreateWireSphere(body, Color.white, (float)customPoi.Diameter(), .01f);
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
            var sphere = AddOrGetSphereComponent(body, GetPrefixName(body, radius));

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
            PurgeCircles();
        }

        private void CreateBodyCircle(CelestialBody body)
        {
            if (!DrawCircles)
            {
                return;
            }

            Log($"[MapOverlay]: Generating circles around {body.name}");
            if (Settings.EnablePOI_HillSphere)
            {
                CreateCircle(body, Color.white, (float)body.hillSphere, 1f);
            }

            var shouldShowMaxAlt =
                Settings.EnablePOI_MaxAlt && (!body.atmosphere || Settings.ShowPOI_MaxAlt_OnAtmoBodies);
            if (shouldShowMaxAlt)
            {
                // TODO: scale sampleRes based on body.Radius
                var maxAlt = body.Radius + Lib.GetApproxTerrainMaxHeight(body);
                CreateCircle(body, Color.red, (float)maxAlt, 1f);
            }

            if (Settings.EnablePOI_SOI)
            {
                CreateCircle(body, Color.magenta, (float)body.sphereOfInfluence, 1f);
            }

            if (Settings.EnablePOI_MinOrbit)
            {
                CreateCircle(body, Color.green, (float)body.minOrbitalDistance, 1f);
            }

            if (body.atmosphere && Settings.EnablePOI_Atmo)
            {
                var atmoDist = body.atmosphereDepth + body.Radius;
                CreateCircle(body, Color.cyan, (float)atmoDist, 1f);
            }

            foreach (var customPoi in Enumerable.Where(_customPois, poi => poi.Enabled() && poi.Diameter() > 0))
            {
                // TODO: custom color and specific body
                CreateCircle(body, Color.white, (float)customPoi.Diameter(), 1f);
            }
        }

        private CircleRenderer CreateCircle(CelestialBody body, Color color, float radius, float width = 1f,
            int segments = 360)
        {
            var circle = AddOrGetCircleComponent(body, GetPrefixName(body, radius));

            circle.wireframeColor = color;
            circle.radius = radius * ScaledSpace.InverseScaleFactor;
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

        private string GetPrefixName(CelestialBody body, double radius)
        {
            return $"{body.name}_{radius}";
        }

        private CircleRenderer AddOrGetCircleComponent(CelestialBody body, string uniqueGameObjectNamePrefix)
        {
            var target = GetOrCreateBodyComponentHolder(body, ComponentHolderType.Circle);
            var components = target.GetComponents<CircleRenderer>();
            foreach (var component in components)
            {
                if (component.IsAlive() && !component.IsDying && component.uniqueGameObjectNamePrefix == uniqueGameObjectNamePrefix)
                {
                    Log($"[AddOrGetCircleComponent] Reusing: {uniqueGameObjectNamePrefix}");
                    return component;
                }

                if(component.IsAlive() && component.IsDying && component.uniqueGameObjectNamePrefix == uniqueGameObjectNamePrefix)
                {
                    Log($"[AddOrGetCircleComponent] Skipping dying component: {uniqueGameObjectNamePrefix}");
                    continue;
                }

                Log("[AddOrGetCircleComponent] Skipping unknown dead component");
            }

            Log($"[AddOrGetCircleComponent] Creating new component {uniqueGameObjectNamePrefix}");
            var circle = target.AddComponent<CircleRenderer>();
            circle.uniqueGameObjectNamePrefix = uniqueGameObjectNamePrefix;
            return circle;
        }

        private WireSphereRenderer AddOrGetSphereComponent(CelestialBody body, string uniqueGameObjectNamePrefix)
        {
            var target = GetOrCreateBodyComponentHolder(body, ComponentHolderType.Sphere);
            var components = target.GetComponents<WireSphereRenderer>();
            foreach (var component in components)
            {
                if (component.IsAlive() && !component.IsDying && component.uniqueGameObjectNamePrefix == uniqueGameObjectNamePrefix)
                {
                    Log($"[AddOrGetSphereComponent] Reusing: {uniqueGameObjectNamePrefix}");
                    return component;
                }

                if(component.IsAlive() && component.IsDying && component.uniqueGameObjectNamePrefix == uniqueGameObjectNamePrefix)
                {
                    Log($"[AddOrGetSphereComponent] Skipping dying component: {uniqueGameObjectNamePrefix}");
                    continue;
                }

                Log("[AddOrGetSphereComponent] Skipping unknown dead component");
            }

            Log($"[AddOrGetSphereComponent] Creating new component {uniqueGameObjectNamePrefix}");
            var sphere = target.AddComponent<WireSphereRenderer>();
            sphere.uniqueGameObjectNamePrefix = uniqueGameObjectNamePrefix;
            return sphere;
        }

        private enum ComponentHolderType
        {
            Sphere,
            Circle,
        }
        private string GetComponentHolderName(CelestialBody body, ComponentHolderType type)
        {
            return $"component_holder_{GetComponentHolderKey(body, type)}";
        }

        private string GetComponentHolderKey(CelestialBody body, ComponentHolderType type)
        {
            return $"{body.name}_{type}";
        }

        private GameObject GetOrCreateBodyComponentHolder(CelestialBody body, ComponentHolderType type)
        {
            var componentHolder = _bodyComponentHolders.TryGet(GetComponentHolderKey(body, type));
            if (componentHolder.isSome && componentHolder.value.IsAlive())
            {
                Log($"[GetOrCreateBodyComponentHolder] Reusing component holder {type} for {body.name}");
                return componentHolder.value;
            }
            if (componentHolder.isSome && !componentHolder.value.IsAlive())
            {
                Log($"[GetOrCreateBodyComponentHolder] Skipping dead component holder for {body.name}");
            }
            Log($"[GetOrCreateBodyComponentHolder] Creating component holder for {body.name}");
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.name = GetComponentHolderName(body, type);
            _bodyComponentHolders.Add(GetComponentHolderKey(body, type), primitive);
            return primitive;
        }

        private string GetComponentHolderName(CelestialBody body)
        {
            return $"component_holder_{body.name}";
        }

        private void PurgeAllByNamePrefix(string namePrefix)
        {
            PurgeAllCirclesByNamePrefix(namePrefix);
            PurgeAllSpheresByNamePrefix(namePrefix);
        }

        private void PurgeAllCirclesByNamePrefix(string namePrefix)
        {
            foreach (var bodyComponentHolder in _bodyComponentHolders)
            {
                var components = bodyComponentHolder.Value.GetComponents<CircleRenderer>().Where(c => c.uniqueGameObjectNamePrefix == namePrefix);;
                Log($"[PurgeAllCirclesByNamePrefix] {bodyComponentHolder.Key} {namePrefix} - {components.Count()}");
                foreach (var component in components)
                {
                    DestroyImmediate(component);
                }
            }
        }

        private void PurgeAllSpheresByNamePrefix(string namePrefix)
        {
            foreach (var bodyComponentHolder in _bodyComponentHolders)
            {
                var components = bodyComponentHolder.Value.GetComponents<WireSphereRenderer>().Where(c => c.uniqueGameObjectNamePrefix == namePrefix);
                Log($"[PurgeAllSpheresByNamePrefix] {bodyComponentHolder.Key} {namePrefix} - {components.Count()}");
                foreach (var component in components)
                {
                    DestroyImmediate(component);
                }
            }
        }

        private void PurgeAllByBody(CelestialBody body)
        {
            PurgeAllCirclesByBody(body);
            PurgeAllSpheresByBody(body);
        }

        private void PurgeAllCirclesByBody(CelestialBody body)
        {
            var bodyComponentHolder = _bodyComponentHolders.TryGet(body.name);
            if (bodyComponentHolder.isNone) return;
            var components = bodyComponentHolder.value.GetComponents<WireSphereRenderer>();
            Log($"[PurgeAllCirclesByBody] {body.name} - {components.Length}");
            foreach (var component in components)
            {
                DestroyImmediate(component);
            }
        }

        private void PurgeAllSpheresByBody(CelestialBody body)
        {
            var bodyComponentHolder = _bodyComponentHolders.TryGet(body.name);
            if (bodyComponentHolder.isNone) return;
            var components = bodyComponentHolder.value.GetComponents<WireSphereRenderer>();
            Log($"[PurgeAllSpheresByBody] {body.name} - {components.Length}");
            foreach (var component in components)
            {
                DestroyImmediate(component);
            }
        }

        // TODO: desperate times call for desperate measures
        internal void PurgeAll()
        {
            Log("=== PURGING ALL ===");

            PurgeSpheres();
            PurgeCircles();
            PurgeBodyHolders();
        }

        private void PurgeBodyHolders()
        {
            Log("=== PURGING BODY HOLDERS ===");
            _bodyComponentHolders.Clear();
            foreach (var body in FlightGlobals.Bodies)
            {
                foreach (ComponentHolderType type in Enum.GetValues(typeof(ComponentHolderType)))
                {
                    DestroyIfAliveGO(GameObject.Find(GetComponentHolderName(body, type)));
                }
            }
        }

        internal void PurgeSpheres()
        {
            Log("=== PURGING SPHERES ===");
            _drawnSpheres.Clear();
            foreach (var component in GameObject.FindObjectsOfType<WireSphereRenderer>())
            {
                DestroyIfAliveMB(component);
            }
            foreach (var lineRenderer in GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == WireSphereRenderer.NameKey))
            {
                DestroyIfAliveGO(lineRenderer);
            }
        }

        internal void PurgeCircles()
        {
            Log("=== PURGING CIRCLES ===");
            _drawnCircles.Clear();
            foreach (var component in GameObject.FindObjectsOfType<CircleRenderer>())
            {
                DestroyIfAliveMB(component);
            }
            foreach (var lineRenderer in GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == CircleRenderer.NameKey))
            {
                DestroyIfAliveGO(lineRenderer);
            }
        }

        private void DestroyIfAliveMB(MonoBehaviour target)
        {
            if (target.IsAlive())
            {
                DestroyImmediate(target);
            }
        }

        private void DestroyIfAliveGO(GameObject target)
        {
            if (target.IsAlive())
            {
                DestroyImmediate(target);
            }
        }
    }

    sealed class CustomPOI
    {
        public Func<bool> Enabled { get; set; }
        public Func<double> Diameter { get; set; }
    }
}
