using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
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
    public class GameStateManager : MonoBehaviour, IHasGameState
    {
        public static GameStateManager Instance { get; private set; }

        private GameState _gameState;
        private Settings _settings;
        private OrbitPoiVisualizer<GameStateManager> _visualizer;

        private bool _eventsRegistered;



        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[GameStateManager] {message}");
        }

        private static void Log(string message)
        {
            Logger.Log($"[GameStateManager] {message}");
        }

        private static void LogError(string message)
        {
            Logger.LogError($"[GameStateManager] {message}");
        }

        public Settings Settings => _settings;
        public GameState GameState => _gameState;
        public OrbitPoiVisualizer<GameStateManager> Visualizer => _visualizer;

        #region Lifecycle Methods

        private IEnumerable<IRenderer> GetRenderReferencesForPoi(POI poi, string tag = "")
        {
            var renderReferences =
                Visualizer.PoiRenderReferenceManager.GetAllRenderReferencesRenderersForPoi(poi);
            var references = Enumerable.ToList(renderReferences);
            if (!references.Any())
            {
                LogError($"{tag}[GetRenderReferencesForPoi] no render references! {Logger.GetPoiLogId(poi)}");
            }
            return references;
        }

        private void Awake()
        {
            LogDebug("Awake");
            Instance = this;
            _gameState = new GameState();
            _visualizer = new OrbitPoiVisualizer<GameStateManager>(this);
            Visualizer.Init();

            _settingsPropChangeMapper = new PropChangeMapper<Settings, OrbitPoiVisualizer<GameStateManager>>(
                PropChangeMapping<Settings, OrbitPoiVisualizer<GameStateManager>>.From(s => s.AlignSpheres, v => v.AlignSpheres),
                PropChangeMapping<Settings, OrbitPoiVisualizer<GameStateManager>>.From(s => s.EnableSpheres, v => v.DrawSpheres, () => Visualizer.SetEnabledSpheres(Visualizer.DrawSpheres)),
                PropChangeMapping<Settings, OrbitPoiVisualizer<GameStateManager>>.From(s => s.EnableCircles, v => v.DrawCircles, () => Visualizer.SetEnabledCircles(Visualizer.DrawCircles)),
                PropChangeMapping<Settings, OrbitPoiVisualizer<GameStateManager>>.From(s => s.FocusedBodyOnly, v => v.FocusedBodyOnly, () => Visualizer.RefreshCurrentRenderers()),
                PropChangeMapping<Settings, OrbitPoiVisualizer<GameStateManager>>.From(s => s.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies, () => Visualizer.RefreshCurrentRenderers())
            );

            _poiPropChangeMapper = new PropChangeActionMapper<POI>(
                PropChangeActionMapping<POI>.From(s => s.Color, (args) =>
                {
                    var poi = args.Source;
                    var sourceColor = poi.Color;
                    LogDebug($"[PropChangeActionMapping:Color] processing Color change for {Logger.GetPoiLogId(poi)} Color: {sourceColor}");
                    foreach (var renderer in GetRenderReferencesForPoi(args.Source, "[PropChangeActionMapping:Color]"))
                    {
                        renderer.SetColor(sourceColor);
                    }
                }),
                PropChangeActionMapping<POI>.From(s => s.LineWidth, (args) =>
                {
                    var poi = args.Source;
                    LogDebug($"[PropChangeActionMapping:LineWidth] processing LineWidth change for {Logger.GetPoiLogId(poi)} LineWidth: {poi.LineWidth}");
                    foreach (var renderer in GetRenderReferencesForPoi(poi, "[PropChangeActionMapping:LineWidth]"))
                    {
                        renderer.SetWidth(args.Source.LineWidth);
                    }
                }),
                PropChangeActionMapping<POI>.From(s => s.Enabled, (args) =>
                {
                    var poi = args.Source;
                    LogDebug($"[PropChangeActionMapping:Enabled] processing Enabled change for {Logger.GetPoiLogId(poi)} Enabled: {poi.Enabled}");
                    // skip custom as they wont need reset
                    if (poi.Type.IsStandard())
                    {
                        LogDebug($"[PropChangeActionMapping:Enabled] resetting standard poi {Logger.GetPoiLogId(poi)}");
                        // TODO: another state hack
                        // this is required to make sure we are using the correct reference when a user configured poi is disabled
                        Visualizer.ResetStandardPoi(poi);
                    }
                    LogDebug($"[PropChangeActionMapping:Enabled] refreshing renderers {Logger.GetPoiLogId(poi)}");
                    Visualizer.RefreshCurrentRenderers();
                }),
                PropChangeActionMapping<POI>.From(s => s.AddPlanetRadius, (args) =>
                {
                    Visualizer.RemovePoi(args.Source);
                    Visualizer.AddPoi(args.Source);
                    Visualizer.CurrentTargetRefresh();
                }),
                PropChangeActionMapping<POI>.From(s => s.Radius, (args) =>
                {
                    Visualizer.RemovePoi(args.Source);
                    Visualizer.AddPoi(args.Source);
                    Visualizer.CurrentTargetRefresh();
                })
            );

            CheckEnabled(Settings.Instance);
        }

        private void OnEnable()
        {
            LogDebug("OnEnable");
            CheckEnabled(Settings.Instance);
            if (enabled) Visualizer.CurrentTargetRefresh();
        }

        private void Start()
        {
            LogDebug("Start");
            CheckEnabled(Settings.Instance);
        }

        private void OnDisable()
        {
            LogDebug("OnDisable");
            // this prevents us from waking up again
            // but instead of leaving a envent handler just for GlobalEnable
            // we'll pass the responsibility to the UI
            UnregisterSettings();
            RegisterEvents(false);
            Visualizer.SetEnabled(false);
        }

        private void OnDestroy()
        {
            LogDebug("OnDestroy");
            UnregisterSettings();
            RegisterEvents(false);
            Visualizer.SetEnabled(false);
            Visualizer.PoiRenderReferenceManager.Clear();
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

        private bool UpdateTargets(MapObject target = null)
        {
            target ??= PlanetariumCamera.fetch.target;
            GameState.CurrentTarget = target;
            var vessel = target.vessel;
            var hasVessel = vessel != null;
            var body = hasVessel ? vessel.mainBody : target.celestialBody;

            if (GameState.FocusedOrActiveVessel == vessel && GameState.FocusedOrActiveBody == body) return false;

            var oldVesselName = MapObjectHelper.GetVesselName(GameState.FocusedOrActiveVessel);
            var newVesselName = MapObjectHelper.GetVesselName(vessel);
            var vesselUpdateText = $"{oldVesselName} -> {newVesselName}";


            var oldBodyName = MapObjectHelper.GetBodyName(GameState.FocusedOrActiveBody);
            var newBodyName = MapObjectHelper.GetBodyName(body);
            var bodyUpdateText = $"{oldBodyName} -> {newBodyName}";
            LogDebug($"[UPDATE] lastVessel: {vesselUpdateText}, lastOrbitingBody: {bodyUpdateText}");
            GameState.FocusedOrActiveVessel = vessel;
            GameState.FocusedOrActiveBody = body;

            return true;

        }

        private void Update()
        {
            if (!SceneHelper.ViewingMapOrTrackingStation || GameState.IsSceneLoading)
            {
                return;
            }

            if (UpdateTargets())
            {
                Visualizer.CurrentTargetRefresh();
            }

            Visualizer.UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(GameState.FocusedOrActiveVessel));
        }

        private void UnregisterSettings()
        {
            if (_settings == null) return;
            Settings.InstanceDestroyed -= OnSettingsDestroyed;
            Settings.InstanceCreated -= OnSettingsCreated;
            if (!_settings.IsDisposed)
            {
                _settings.PropertyChanged -= OnPropertyChanged;
                _settings.ConfiguredPoiPropChanged -= OnConfiguredPoiChanged;
                _settings.ConfiguredPoisCollectionChanged -= OnConfiguredPoisCollectionChanged;
            }
            _settings = null;
        }

        private void RegisterSettings(Settings settings)
        {
            var sameSettings = ReferenceEquals(_settings, settings);
            if (sameSettings) return;
            UnregisterSettings();
            _settings = settings;
            Settings.InstanceDestroyed += OnSettingsDestroyed;
            Settings.InstanceCreated += OnSettingsCreated;
            _settings.PropertyChanged += OnPropertyChanged;
            _settings.ConfiguredPoiPropChanged += OnConfiguredPoiChanged;
            _settings.ConfiguredPoisCollectionChanged += OnConfiguredPoisCollectionChanged;
        }

        #endregion

        #region EventHandlers


        private void OnSettingsDestroyed(Settings settings)
        {
            LogDebug($"[OnSettingsDestroyed]");
            UnregisterSettings();
        }
        private void OnSettingsCreated(Settings settings)
        {
            LogDebug($"[OnSettingsCreated]");
            RegisterSettings(settings);
            UpdatePropsFromSettings(settings);
        }

        private PropChangeMapper<Settings, OrbitPoiVisualizer<GameStateManager>> _settingsPropChangeMapper;
        private PropChangeActionMapper<POI> _poiPropChangeMapper;

        private void OnPropertyChanged(object senderSettings, PropertyChangedEventArgs args)
        {

            Logger.LogPropertyChange<Settings>(senderSettings, args, "[OnPropertyChanged]");
            if (senderSettings is not Settings settings)
            {
                LogError($"[OnPropertyChanged] {nameof(senderSettings)} is not of type {nameof(Settings)}");
                return;
            }

            if (args.PropertyName == nameof(Settings.ConfiguredPois))
            {
                LogDebug($"[OnPropertyChanged] ignoring Settings.{args.PropertyName}");
                return;
            }

            LogDebug($"[OnPropertyChanged] processing change Settings.{args.PropertyName}");
            UpdatePropsFromSettings(settings);
            _settingsPropChangeMapper.Process(settings, _visualizer, args);
        }

        private void OnConfiguredPoiChanged(object senderSettings, object senderPoi, PropertyChangedEventArgs args)
        {
            Logger.LogPropertyChange<POI>(senderPoi, args, "[OnConfiguredPoiChanged]");
            if (senderPoi is not POI poi)
            {
                LogError($"[OnConfiguredPoiChanged] {nameof(senderPoi)} is not of type {nameof(POI)}");
                return;
            }

            LogDebug($"[OnConfiguredPoiChanged] processing change {Logger.GetPoiLogId(poi)}");
            _poiPropChangeMapper.Process(poi, args);
        }

        private void OnConfiguredPoisCollectionChanged(object settings, NotifyCollectionChangedEventArgs args)
        {
            LogDebug($"[OnConfiguredPoisCollectionChanged] Settings.{nameof(Settings.ConfiguredPois)} - {args.Action}");
            if (args.OldItems != null)
            {
                foreach (var oldItem in args.OldItems)
                {
                    if (oldItem is not POI poi) continue;
                    // don't remove defaults or else they will disappear until a change is made
                    if (Settings.IsDefaultPoi(poi))
                    {
                        LogDebug($"[OnConfiguredPoisCollectionChanged] poi returned to default state (resetting) {Logger.GetPoiLogId(poi)}");
                        var defaultOrConfiguredPois = poi.IsGlobal()
                            ? FlightGlobals.Bodies.Select(body => Settings.GetConfiguredOrDefaultPoiFor(body, poi.Type))
                            : new List<POI>() { Settings.GetConfiguredOrDefaultPoiFor(poi.Body, poi.Type) };

                        foreach (var defaultOrConfiguredPoi in defaultOrConfiguredPois)
                        {
                            LogDebug($"[OnConfiguredPoisCollectionChanged] resetting: {Logger.GetPoiLogId(defaultOrConfiguredPoi)}");
                            Visualizer.ResetStandardPoi(defaultOrConfiguredPoi);
                        }
                        continue;
                    }
                    LogDebug($"[OnConfiguredPoisCollectionChanged] removing {Logger.GetPoiLogId(poi)}");
                    Visualizer.RemovePoi(poi);
                }
            }

            if (args.NewItems != null)
            {
                foreach (var newItem in args.NewItems)
                {
                    if (newItem is not POI poi) continue;
                    LogDebug($"[OnConfiguredPoisCollectionChanged] adding {Logger.GetPoiLogId(poi)}");
                    Visualizer.AddPoi(poi);
                }
            }
            Visualizer.CurrentTargetRefresh();
        }

        private void OnGameSceneLoadRequested(GameScenes scenes)
        {
            GameState.IsSceneLoading = true;
            LogDebug($"[OnGameSceneLoadRequested] {SceneHelper.GetSceneName(scenes)}");
            Visualizer.RemoveAll();
        }

        private void OnGameSceneLoadedGUIReady(GameScenes scenes)
        {
            LogDebug($"[OnGameSceneLoadedGUIReady] {SceneHelper.GetSceneName(scenes)}");
            // TOD: this might not be the same on all systems
            StartCoroutine(DelayedAction.CreateCoroutine(() =>
                {
                    LogDebug($"[OnGameSceneLoadedGUIReady][DelayedAction] {SceneHelper.GetSceneName(scenes)}");
                    GameState.IsSceneLoading = false;
                    if (!SceneHelper.ViewingMapOrTrackingStation)
                    {
                        LogDebug("[OnGameSceneLoadedGUIReady] not in map or tracking - hiding");
                        Visualizer.SetEnabled(false);
                        return;
                    }

                    Visualizer.CurrentTargetRefresh();
                },
                6 // appears it takes 6 frames on my system before OnMapExit is called
            ));
        }

        private void OnMapEntered()
        {
            UpdateTargets();
            LogDebug($"[OnMapEntered] focus on {MapObjectHelper.GetTargetName(GameState.CurrentTarget)}");
            Visualizer.SetEnabled(true);
            Visualizer.CurrentTargetRefresh();
        }

        private void OnMapExited()
        {
            LogDebug($"[OnMapExited]");
            Visualizer.SetEnabled(false);
        }

        private void OnVesselSOIChange(GameEvents.HostedFromToAction<Vessel, CelestialBody> data)
        {
            LogDebug($"[OnVesselSOIChange] soi changed: {data.from.name} -> {data.to.name}");
            if (GameState.IsSceneLoading || !SceneHelper.ViewingMapOrTrackingStation) return;
            UpdateTargets();
            Visualizer.CurrentTargetRefresh();
        }

        private void OnVesselChange(Vessel vessel)
        {
            if (GameState.IsSceneLoading || !SceneHelper.ViewingMapOrTrackingStation) return;
            UpdateTargets(vessel.mapObject);
            Visualizer.CurrentTargetRefresh();
            Visualizer.UpdateNormals(MapObjectHelper.GetVesselOrbitNormal(vessel));
        }

        private void OnMapFocusChange(MapObject focusTarget)
        {
            if (GameState.IsSceneLoading || !SceneHelper.ViewingMapOrTrackingStation) return;
            LogDebug($"[OnMapFocusChange] Changed focus to {focusTarget.name}");
            // TODO: this gets called when loading a save and we dont want to generate anything unless in map
            UpdateTargets(focusTarget);
            Visualizer.CurrentTargetRefresh();
        }

        #endregion


        private void UpdatePropsFromSettings(Settings settings)
        {
            enabled = settings.GlobalEnable;
            Visualizer.DrawCircles = settings.EnableCircles;
            Visualizer.DrawSpheres = settings.EnableSpheres;
            Visualizer.AlignSpheres = settings.AlignSpheres;
            Visualizer.FocusedBodyOnly = settings.FocusedBodyOnly;
        }

        public void SetEnabled(bool state)
        {
            Visualizer.SetEnabled(state);
            enabled = state;
        }

        private void CheckEnabled(Settings settings)
        {
            RegisterSettings(settings);
            UpdatePropsFromSettings(settings);
            LogDebug($"[CheckEnabled] enable: {settings.GlobalEnable}, circles: {Visualizer.DrawCircles}, spheres: {Visualizer.DrawSpheres}, align spheres: {Visualizer.AlignSpheres}");
            // check to make sure we still enabled after loading settings
            if (!enabled)
            {
                Visualizer.SetEnabled(false);
                return;
            }
            var canDraw = !GameState.IsSceneLoading && SceneHelper.ViewingMapOrTrackingStation;
            Visualizer.SetEnabled(canDraw);
            RegisterEvents();
        }

    }
}
