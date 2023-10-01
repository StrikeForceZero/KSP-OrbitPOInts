using System;
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

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class OrbitPoiVisualizer : MonoBehaviour
    {
        public static OrbitPoiVisualizer Instance { get; private set; }

        private Settings _settings;

        private readonly Dictionary<string, GameObject> _bodyComponentHolders = new();
        private readonly HashSet<WireSphereRenderer> _drawnSpheres = new();
        private readonly HashSet<CircleRenderer> _drawnCircles = new();

        public bool DrawSpheres;
        public bool DrawCircles;
        public bool AlignSpheres;

        private bool _eventsRegistered;
        private static bool _sceneLoading;

        private Vessel _lastVessel;
        private CelestialBody _lastOrbitingBody;

        private static double _standardLineWidthDistance;

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

        public void SetEnabled(bool state)
        {
            foreach (var (sphere, index) in _drawnSpheres.Select((value, index) => (value, index)))
            {
                if (!sphere.IsAlive() || sphere.IsDying)
                {
                    LogError($"[SetEnabled] sphere null or dying {index} / {_drawnSpheres.Count}");
                    continue;
                }
                sphere.SetEnabled(state);
            }

            foreach (var (circle, index) in _drawnCircles.Select((value, index) => (value, index)))
            {
                if (!circle.IsAlive() || circle.IsDying)
                {
                    LogError($"[SetEnabled] circle null or dying {index} / {_drawnCircles.Count}");
                    continue;
                }
                circle.SetEnabled(state);
            }

            enabled = state;
        }

        #region Lifecycle Methods

        private void Awake()
        {
            LogDebug("Awake");
            LoadStandardLineWidthDistance();
            Instance = this;
            CheckEnabled();
        }

        private void OnEnable()
        {
            LogDebug("OnEnable");
            CheckEnabled();
        }

        private void Start()
        {
            LogDebug("Start");
            CheckEnabled();
        }

        private void OnDisable()
        {
            LogDebug("OnDisable");
            // this prevents us from waking up again
            // but instead of leaving a envent handler just for GlobalEnable
            // we'll pass the responsibility to the UI
            UnregisterSettings();
            RegisterEvents(false);
            PurgeAll();
        }

        private void OnDestroy()
        {
            LogDebug("OnDestroy");
            UnregisterSettings();
            RegisterEvents(false);
            PurgeAll();
        }

        private void RegisterEvents(bool register = true)
        {
            if (register)
            {
                if (_eventsRegistered) return;
                LogDebug("RegisterEvents");
                GameEvents.OnMapEntered.Add(OnMapEntered);
                GameEvents.OnMapExited.Add(OnMapExited);
                GameEvents.OnMapFocusChange.Add(OnMapFocusChange);
                GameEvents.onVesselChange.Add(OnVesselChange);
                GameEvents.onVesselSOIChanged.Add(OnVesselSOIChange);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
                GameEvents.onLevelWasLoadedGUIReady.Add(OnGameSceneLoadedGUIReady);
                _eventsRegistered = true;
                return;
            }

            if (!_eventsRegistered) return;
            LogDebug("UnRegisterEvents");
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.OnMapExited.Remove(OnMapExited);
            GameEvents.OnMapFocusChange.Remove(OnMapFocusChange);
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChange);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.onLevelWasLoadedGUIReady.Remove(OnGameSceneLoadedGUIReady);
            _eventsRegistered = false;
        }

        private void Update()
        {
            if (!SceneHelper.ViewingMapOrTrackingStation)
            {
                return;
            }

            var target = PlanetariumCamera.fetch.target;
            var vessel = target.vessel;
            var hasVessel = vessel != null;
            var body = hasVessel ? vessel.mainBody : target.celestialBody;

            if (_lastVessel != vessel || _lastOrbitingBody != body)
            {
                LogDebug(
                    $"[UPDATE] lastVessel: {MapObjectHelper.GetVesselName(_lastVessel)} -> {MapObjectHelper.GetVesselName(vessel)}, lastOrbitingBody: {MapObjectHelper.GetBodyName(_lastOrbitingBody)} -> {MapObjectHelper.GetBodyName(body)}");
                Refresh(target);

                _lastVessel = vessel;
                _lastOrbitingBody = body;
            }

            UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(vessel));
        }

        private void UnregisterSettings()
        {
            if (_settings == null) return;
            Settings.InstanceDestroyed -= OnSettingsDestroyed;
            Settings.InstanceCreated -= OnSettingsCreated;
            _settings.PropertyChanged -= OnPropertyChanged;
            _settings.ConfiguredPoiPropChanged -= OnConfiguredPoisChanged;
            _settings.ConfiguredPoisCollectionChanged -= OnConfiguredPoisCollectionChanged;
            _settings = null;
        }

        private void RegisterSettings()
        {
            if (_settings is { IsDisposed: false }) return;
            Settings.InstanceDestroyed -= OnSettingsDestroyed;
            Settings.InstanceCreated -= OnSettingsCreated;
            _settings = Settings.Instance;
            Settings.InstanceDestroyed += OnSettingsDestroyed;
            Settings.InstanceCreated += OnSettingsCreated;
            _settings.PropertyChanged += OnPropertyChanged;
            // remove and reregister at the end because we want to make sure we at least got the event that the collection was reset
            _settings.ConfiguredPoiPropChanged -= OnConfiguredPoisChanged;
            _settings.ConfiguredPoiPropChanged += OnConfiguredPoisChanged;
            _settings.ConfiguredPoisCollectionChanged -= OnConfiguredPoisCollectionChanged;
            _settings.ConfiguredPoisCollectionChanged -= OnConfiguredPoisCollectionChanged;
        }

        #endregion

        #region EventHandlers


        private void OnSettingsDestroyed(Settings settings)
        {
            UnregisterSettings();
        }
        private void OnSettingsCreated(Settings settings)
        {
            RegisterSettings();
            UpdatePropsFromSettings();
            // maybe
            // CurrentTargetRefresh();
        }

        private void OnPropertyChanged(object settings, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Settings.ConfiguredPois)) return;

            UpdatePropsFromSettings();
            CurrentTargetRefresh();
        }

        private void OnConfiguredPoisChanged(object settings, object poi, PropertyChangedEventArgs args)
        {
            CurrentTargetRefresh();
        }

        private void OnConfiguredPoisCollectionChanged(object settings, NotifyCollectionChangedEventArgs args)
        {
            CurrentTargetRefresh();
        }

        private void OnGameSceneLoadRequested(GameScenes scenes)
        {
            _sceneLoading = true;
            LogDebug($"[OnGameSceneLoadRequested] {SceneHelper.GetSceneName(scenes)}");
            RemoveAll();
        }

        private void OnGameSceneLoadedGUIReady(GameScenes scenes)
        {
            LogDebug($"[OnGameSceneLoadedGUIReady] {SceneHelper.GetSceneName(scenes)}");
            // TOD: this might not be the same on all systems
            StartCoroutine(DelayedAction.CreateCoroutine(() =>
                {
                    LogDebug($"[OnGameSceneLoadedGUIReady][DelayedAction] {SceneHelper.GetSceneName(scenes)}");
                    _sceneLoading = false;
                    if (PurgeIfNotInMapOrTracking())
                    {
                        LogDebug("[OnGameSceneLoadedGUIReady] purge complete");
                        return;
                    }

                    CurrentTargetRefresh();
                },
                6 // appears it takes 6 frames on my system before OnMapExit is called
            ));
        }

        private void OnMapEntered()
        {
            var target = PlanetariumCamera.fetch.target;
            LogDebug($"[OnMapEntered] focus on {MapObjectHelper.GetTargetName(target)}");

            Refresh(target);
        }

        private void OnMapExited()
        {
            LogDebug($"[OnMapExited]");
            PurgeAll();
        }

        private void OnVesselSOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> data)
        {
            LogDebug($"[OnVesselSOIChange] soi changed: {data.from.name} -> {data.to.name}");
            if (PurgeIfNotInMapOrTracking())
            {
                // TODO: this might be ok if our SOI changes while flying
                LogError("[OnVesselSOIChange] OnVesselSOIChange called when not in map or tracking!");
                return;
            }
            UpdateBody(data.to);
        }

        private void OnVesselChange(Vessel vessel)
        {
            if (_sceneLoading || !SceneHelper.ViewingMapOrTrackingStation) return;
            LogDebug(
                $"[OnVesselChange] vessel changed: {MapObjectHelper.GetVesselName(_lastVessel)} -> {MapObjectHelper.GetVesselName(vessel)}");

            if (PurgeIfNotInMapOrTracking())
            {
                // TODO: this might be ok if we are flightview and switch ships
                LogError("[OnVesselChange] OnVesselChange called when not in map or tracking!");
                return;
            }

            if (vessel == null)
            {
                return;
            }

            UpdateBody(vessel.mainBody);
            UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(vessel));
        }

        private void OnMapFocusChange(MapObject focusTarget)
        {
            if (_sceneLoading || !SceneHelper.ViewingMapOrTrackingStation) return;
            LogDebug($"[OnMapFocusChange] Changed focus to {focusTarget.name}");
            // TODO: this gets called when loading a save and we dont want to generate anything unless in map
            if (PurgeIfNotInMapOrTracking())
            {
                LogError("[OnMapFocusChange] OnMapFocusChange called when not in map or tracking!");
                return;
            }
            Refresh(focusTarget);
        }

        #endregion

        #region Refresh

        public void UpdateBody(CelestialBody body)
        {
            LogDebug($"[UpdateBody] body: {body.name}");
            PurgeAll();
            if (!enabled)
            {
                LogError("[UpdateBody] UpdateBody called when not enabled!");
                return;
            }
            if (!SceneHelper.ViewingMapOrTrackingStation)
            {
                LogError("[UpdateBody] UpdateBody called not ViewingMapOrTrackingStation!");
                return;
            }
            if (_sceneLoading)
            {
                LogError("[UpdateBody] UpdateBody called when scene loading!");
                return;
            }

            if (_settings.FocusedBodyOnly)
            {
                DestroyAndRecreateBodySpheres(body);
                DestroyAndRecreateBodyCircles(body);
                return;
            }

            // TODO: this is more of a bad hack since we are still using purge methods
            foreach (var curBody in FlightGlobals.Bodies)
            {
                CreateBodySphere(curBody);
                CreateBodyCircle(curBody);
            }
        }

        public void Refresh(MapObject focusTarget)
        {
            if (!enabled)
            {
                LogError("[Refresh] refresh called when not enabled!");
                return;
            }
            if (PurgeIfNotInMapOrTracking())
            {
                LogError("[Refresh] refresh called when not in map or tracking!");
                return;
            }
            if (_sceneLoading)
            {
                LogError("[Refresh] refresh called when scene loading!");
                return;
            }

            if (focusTarget == null)
            {
                LogError("[Refresh] target is null!");
                PurgeAll();
                return;
            }

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
            PurgeAll();
        }

        #endregion

        #region Normals
        private void UpdateNormals(Vector3 normal)
        {
            if (!enabled || _sceneLoading || !SceneHelper.ViewingMapOrTrackingStation)
            {
                return;
            }
            List<Transform> transformsNeedsUpdate = new();
            foreach (var (circle, index) in _drawnCircles.Select((value, index) => (value, index)))
            {
                if (!circle.IsAlive() || circle.IsDying)
                {
                    LogError($"[UpdateNormals] circle null or dying {index} / {_drawnCircles.Count}");
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
                        LogError($"[UpdateNormals] sphere null or dying {index} / {_drawnSpheres.Count}");
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
                Logger.LogError($"[NextFrameAlignTransformToNormal] transform null!");
                return;
            }

            StartCoroutine(DelayedAction.CreateCoroutine(() => AlignTransformToNormal(transform, normal), 1));
        }

        private void AlignTransformToNormal(Transform transform, Vector3d normal)
        {
            // TODO: sometimes the transform can be null
            if (transform == null)
            {
                Logger.LogError($"[AlignTransformToNormal] transform null!");
                return;
            }
            if (!enabled)
            {
                return;
            }
            OrbitHelpers.AlignTransformToNormal(transform, normal);
        }
        #endregion

        private void CreatePoisForBody(CelestialBody body, Action<POI> onCreatePoi)
        {
            foreach (PoiType poiType in Enum.GetValues(typeof(PoiType)))
            {
                if (poiType is PoiType.None or PoiType.Custom) continue;
                // check to make sure its not disabled in the global config
                if (!Settings.Instance.GetGlobalEnableFor(body, poiType))
                {
                    LogDebug($"[CreatePoisForBody] skipping - global disable for body:{body.Serialize()} type:{poiType}");
                    continue;
                }
                var poi = Settings.Instance.GetStandardPoiFor(body, poiType);
                switch (poiType)
                {
                    case PoiType.MaxTerrainAltitude when !body.atmosphere && !Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies:
                    case PoiType.Atmosphere when !body.atmosphere:
                        LogDebug($"[CreatePoisForBody] skipping body:{body.Serialize()} type:{poiType} - atmosphere:{body.atmosphere} ShowPoiMaxTerrainAltitudeOnAtmosphericBodies:{Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies}");
                        continue;
                    default:
                        LogDebug($"[CreatePoisForBody] body:{poi.Body.bodyName} type:{poi.Type} renderRadius:{poi.RadiusForRendering()} color:{poi.Color.Serialize()}");
                        onCreatePoi.Invoke(poi);
                        break;
                }
            }

            var customPois = Settings.Instance.GetCustomPoisFor(body);
            foreach (var customPoi in customPois.Where(poi => poi.Enabled && poi.RadiusForRendering() > 0))
            {
                LogDebug($"[CreatePoisForBody] body:{customPoi.Body.bodyName} type:{customPoi.Type} renderRadius:{customPoi.RadiusForRendering()} color:{customPoi.Color.Serialize()}");
                onCreatePoi.Invoke(customPoi);
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

            LogDebug($"[CreateBodySphere]: Generating spheres around {body.name}");
            CreatePoisForBody(body, poi => CreateWireSphereFromPoi(poi));
        }

        private WireSphereRenderer CreateWireSphereFromPoi(POI poi)
        {
            LogDebug($"[CreateWireSphereFromPoi]: Generating spheres around body: {poi.Body.Serialize()}, color:{poi.Color.Serialize()}, radius:{poi.RadiusForRendering()}, line:{poi.LineWidth}, res: {poi.Resolution}");
            return CreateWireSphere(poi.Body, poi.Color, (float)poi.RadiusForRendering(), poi.LineWidth, poi.Resolution);
        }

        private WireSphereRenderer CreateWireSphere(
            CelestialBody body,
            Color color,
            float radius,
            float width = 1f,
            int resolution = 50
        )
        {
            var sphere = AddOrGetSphereComponent(body, GetComponentPrefixName(body, radius));

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

            LogDebug($"[CreateBodyCircle]: Generating circles around {body.name}");
            CreatePoisForBody(body, poi => CreateCircleFromPoi(poi));
        }

        private CircleRenderer CreateCircleFromPoi(POI poi)
        {
            LogDebug($"[CreateCircleFromPoi]: Generating circle around body: {poi.Body.Serialize()}, color:{poi.Color.Serialize()}, radius:{poi.RadiusForRendering()}, line:{poi.LineWidth}, res: {poi.Resolution}");
            return CreateCircle(poi.Body, poi.Color, (float)poi.RadiusForRendering(), poi.LineWidth);
        }

        private CircleRenderer CreateCircle(CelestialBody body, Color color, float radius, float width = 1f,
            int segments = 360)
        {
            var circle = AddOrGetCircleComponent(body, GetComponentPrefixName(body, radius));

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

        #region MISC

        private void UpdatePropsFromSettings()
        {
            enabled = Settings.Instance.GlobalEnable;
            DrawCircles = Settings.Instance.EnableCircles;
            DrawSpheres = Settings.Instance.EnableSpheres;
            AlignSpheres = Settings.Instance.AlignSpheres;
        }

        private void CheckEnabled()
        {
            RegisterSettings();
            UpdatePropsFromSettings();
            LogDebug($"[CheckEnabled] enable: {Settings.Instance.GlobalEnable}, circles: {DrawCircles}, spheres: {DrawSpheres}, align spheres: {AlignSpheres}");
            // check to make sure we still enabled after loading settings
            if (!enabled)
            {
                PurgeAll();
                return;
            }

            PurgeIfNotInMapOrTracking();
            RegisterEvents();
        }

        private bool PurgeIfNotInMapOrTracking()
        {
            // if for some reason we end up here and we arent in the mapview or tracking station we should purge
            if (!SceneHelper.ViewingMapOrTrackingStation)
            {
                LogDebug("[PurgeIfNotInMapOrTracking] Purging");
                PurgeAll();
                return true;
            }

            return false;
        }

        private static void LoadStandardLineWidthDistance()
        {
            foreach (var body in FlightGlobals.Bodies.Where(body => body.isHomeWorld))
            {
                _standardLineWidthDistance = body.minOrbitalDistance;
                break;
            }
        }
        #endregion

        #region Components
        private string GetComponentPrefixName(CelestialBody body, double radius)
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
                    LogDebug($"[AddOrGetCircleComponent] Reusing: {uniqueGameObjectNamePrefix}");
                    return component;
                }

                if(component.IsAlive() && component.IsDying && component.uniqueGameObjectNamePrefix == uniqueGameObjectNamePrefix)
                {
                    LogDebug($"[AddOrGetCircleComponent] Skipping dying component: {uniqueGameObjectNamePrefix}");
                    continue;
                }

                LogDebug("[AddOrGetCircleComponent] Skipping unknown dead component");
            }

            LogDebug($"[AddOrGetCircleComponent] Creating new component {uniqueGameObjectNamePrefix}");
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
                    LogDebug($"[AddOrGetSphereComponent] Reusing: {uniqueGameObjectNamePrefix}");
                    return component;
                }

                if(component.IsAlive() && component.IsDying && component.uniqueGameObjectNamePrefix == uniqueGameObjectNamePrefix)
                {
                    LogDebug($"[AddOrGetSphereComponent] Skipping dying component: {uniqueGameObjectNamePrefix}");
                    continue;
                }

                LogDebug("[AddOrGetSphereComponent] Skipping unknown dead component");
            }

            LogDebug($"[AddOrGetSphereComponent] Creating new component {uniqueGameObjectNamePrefix}");
            var sphere = target.AddComponent<WireSphereRenderer>();
            sphere.uniqueGameObjectNamePrefix = uniqueGameObjectNamePrefix;
            return sphere;
        }

        #region Component Holder
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
            if (componentHolder.IsSome && componentHolder.Value.IsAlive())
            {
                LogDebug($"[GetOrCreateBodyComponentHolder] Reusing component holder {type} for {body.name}");
                return componentHolder.Value;
            }
            if (componentHolder.IsSome && !componentHolder.Value.IsAlive())
            {
                LogDebug($"[GetOrCreateBodyComponentHolder] Skipping dead component holder for {body.name}");
            }
            LogDebug($"[GetOrCreateBodyComponentHolder] Creating component holder for {body.name}");
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.name = GetComponentHolderName(body, type);
            _bodyComponentHolders.Add(GetComponentHolderKey(body, type), primitive);
            return primitive;
        }
        #endregion
        #endregion

        #region PurgeMethods
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
                LogDebug($"[PurgeAllCirclesByNamePrefix] {bodyComponentHolder.Key} {namePrefix} - {components.Count()}");
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
                LogDebug($"[PurgeAllSpheresByNamePrefix] {bodyComponentHolder.Key} {namePrefix} - {components.Count()}");
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
            if (bodyComponentHolder.IsNone) return;
            var components = bodyComponentHolder.Value.GetComponents<WireSphereRenderer>();
            LogDebug($"[PurgeAllCirclesByBody] {body.name} - {components.Length}");
            foreach (var component in components)
            {
                DestroyImmediate(component);
            }
        }

        private void PurgeAllSpheresByBody(CelestialBody body)
        {
            var bodyComponentHolder = _bodyComponentHolders.TryGet(body.name);
            if (bodyComponentHolder.IsNone) return;
            var components = bodyComponentHolder.Value.GetComponents<WireSphereRenderer>();
            LogDebug($"[PurgeAllSpheresByBody] {body.name} - {components.Length}");
            foreach (var component in components)
            {
                DestroyImmediate(component);
            }
        }

        // TODO: desperate times call for desperate measures
        internal void PurgeAll()
        {
            LogDebug("=== PURGING ALL ===");

            PurgeSpheres();
            PurgeCircles();
            PurgeBodyHolders();
        }

        private void PurgeBodyHolders()
        {
            LogDebug("=== PURGING BODY HOLDERS ===");
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
            LogDebug("=== PURGING SPHERES ===");
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
            LogDebug("=== PURGING CIRCLES ===");
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
        #endregion

        #region DestroyHelpers
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
        #endregion
    }
}
