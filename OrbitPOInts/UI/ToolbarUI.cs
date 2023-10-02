using System;
using System.Collections.Generic;
using System.Globalization;
using OrbitPOInts.Data;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;
using OrbitPOInts.Extensions.Unity;
using OrbitPOInts.Helpers;
using OrbitPOInts.Utils;

#if TEST
using KSPMock.UI.Screens;
using UnityEngineMock;
using KSP_GameDatabase = KSPMock.GameDatabase;
using KSP_KSPAddon = KSPMock.KSPAddon;
using KSP_GameEvents = KSPMock.GameEvents;
using KSP_HighLogic = KSPMock.HighLogic;
using KSP_GameScenes = KSPMock.GameScenes;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
using KSP_MapView = KSPMock.MapView;
using System.Linq;
#else
using KSP.UI.Screens;
using UniLinq;
using UnityEngine;
using KSP_GameDatabase = GameDatabase;
using KSP_KSPAddon = KSPAddon;
using KSP_GameEvents = GameEvents;
using KSP_HighLogic = HighLogic;
using KSP_GameScenes = GameScenes;
using KSP_CelestialBody = CelestialBody;
using KSP_FlightGlobals = FlightGlobals;
using KSP_MapView = MapView;
#endif

namespace OrbitPOInts.UI
{
    using GameDatabase = KSP_GameDatabase;
    using KSPAddon = KSP_KSPAddon;
    using GameEvents = KSP_GameEvents;
    using HighLogic = KSP_HighLogic;
    using GameScenes = KSP_GameScenes;
    using CelestialBody = KSP_CelestialBody;
    using FlightGlobals = KSP_FlightGlobals;
    using MapView = KSP_MapView;

    using Logger = Utils.Logger;

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ToolbarUI : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private bool showUI = false;
        private Rect windowRect = new Rect(0, 0, 400, 200); // Initial size for the window
        private bool _useTopRightCloseButton = false;
        private bool _eventsRegistered;

        private const string GlobalConfigKey = "Global";
        private IList<string> _selectableBodyNames = new List<string> { GlobalConfigKey };
        private int _selectedBodyIndex = 0;

        private SimpleColorPicker _colorPicker = new SimpleColorPicker();
        private OptionsPopup _optionsPopup = new OptionsPopup();

        private void LogDebug(string message)
        {
            Logger.LogDebug($"[ToolbarUI] {message}");
        }
        
        private void Log(string message)
        {
            Logger.Log($"[ToolbarUI] {message}");
        }

        private void Awake()
        {
            LogDebug("[Awake]");
            RegisterEvents();
            FixState();
        }

        private void Start()
        {
            LogDebug("[Start]");
            RegisterEvents();
            FixState();
        }

        private void OnEnable()
        {
            LogDebug("[OnEnable]");
            RegisterEvents();
            FixState();
        }

        private void OnDisable()
        {
            LogDebug("[OnDisable]");
            Cleanup();
        }

        private void Cleanup()
        {
            RegisterEvents(false);
            RemoveToolbarButton();
        }

        private void RegisterEvents(bool register = true)
        {
            if (register)
            {
                if (_eventsRegistered) return;
                LogDebug("RegisterEvents");
                GameEvents.OnMapEntered.Add(OnMapEntered);
                GameEvents.OnMapExited.Add(OnMapExited);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
                _eventsRegistered = true;
                return;
            }

            if (!_eventsRegistered) return;
            LogDebug("UnRegisterEvents");
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.OnMapExited.Remove(OnMapExited);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            _eventsRegistered = false;
        }

        private void FixState()
        {
            if (_selectableBodyNames.Count == 1)
            {
                foreach (var body in FlightGlobals.Bodies)
                {
                    _selectableBodyNames.Add(body.name);
                }
            }

            LogDebug($"[FixState] ViewingMapOrTrackingStation: {SceneHelper.ViewingMapOrTrackingStation} scene: {SceneHelper.GetSceneName(HighLogic.LoadedScene)}");
            if (!SceneHelper.ViewingMapOrTrackingStation)
            {
                if (RemoveToolbarButton())
                {
                    LogDebug("[FixState] Remove button");
                }
                return;
            }

            if (CreateToolbarButton())
            {
                LogDebug("[FixState] Add button");
            }
        }

        private bool CreateToolbarButton()
        {
            if (toolbarButton) return false;
            toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                OnToolbarButtonClick,
                OnToolbarButtonClick,
                null,
                null,
                null,
                null,
                ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
                GameDatabase.Instance.GetTexture("OrbitPOInts/UI/toolbar_icon", false)
            );
            return true;
        }

        private bool RemoveToolbarButton()
        {
            if (!toolbarButton) return false;
            CloseWindow();
            ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
            return true;
        }

        private void OnToolbarButtonClick()
        {
            showUI = !showUI;
            CenterWindowPos();
        }

        private void OnMapEntered()
        {
            LogDebug("[OnMapEntered]");
            FixState();
        }

        private void OnMapExited()
        {
            LogDebug("[OnMapExited]");
            FixState();
        }

        private void OnGameSceneLoadRequested(GameScenes scenes)
        {
            LogDebug($"[OnGameSceneLoadRequested] {SceneHelper.GetSceneName(scenes)}");
            if (scenes == GameScenes.TRACKSTATION || scenes == GameScenes.FLIGHT && MapView.MapIsEnabled)
            {
                CreateToolbarButton();
                return;
            }
            FixState();
        }

        private void CenterWindowPos()
        {
            if (showUI)
            {
                // Calculate the screen's center
                float centerX = Screen.width / 2;
                float centerY = Screen.height / 2;

                // Adjust for the window's size to get the top-left position
                windowRect.x = centerX - (windowRect.width / 2);
                windowRect.y = centerY - (windowRect.height / 2);
            }
        }


        private void OnGUI()
        {
            if (showUI)
            {
                GUI.skin = Settings.Instance.UseSkin ? HighLogic.Skin : null;
                windowRect = GUILayout.Window(12345, windowRect, DrawUI, "OrbitPOInts");
                _colorPicker.OnGUI();
                _optionsPopup.OnGUI();
            }
        }

        // TODO: needs moar abstraction!
        private void CustomPoiHandler(POI poi)
        {
            // save the old values for checking later
            var oldPoiRadius = poi.Radius;
            var customPoiRadiusInput = TextFieldWithToggle(poi.Enabled, poi.Label, oldPoiRadius.ToString("N", CultureInfo.CurrentCulture));
            var result1 = double.TryParse(customPoiRadiusInput.Text, out var newPoiRadius);
            // ensure value is always positive
            if (result1) poi.Radius = Math.Abs(newPoiRadius);
            newPoiRadius = poi.Radius;
            // check if the values were changed
            var poiChanged = !newPoiRadius.AreRelativelyEqual(oldPoiRadius);
            poi.Enabled = newPoiRadius > 0 && (poiChanged || customPoiRadiusInput.Enabled);
            customPoiRadiusInput.Text = newPoiRadius.ToString("N", CultureInfo.CurrentCulture);

            poi.AddPlanetRadius = GUILayout.Toggle(poi.AddPlanetRadius, "+ PR", GUILayout.ExpandWidth(false));
            GUILayout.Space(50);

            if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
            {
                LogDebug($"[GUI] Remove poi clicked: {poi.Label}");
                Settings.Instance.RemoveConfiguredPoi(poi, true);
            }
        }

        private void CustomPoiGUI(POI poi)
        {
            PoiContainer(
                poi,
                () =>
                {
                    CustomPoiHandler(poi);
                }
            );
        }

        private string TextFieldWithLabel(string label, string text = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            var result = GUILayout.TextField(text);
            GUILayout.EndHorizontal();
            return result;
        }

        private ToggleTextFieldResult TextFieldWithToggle(bool toggled, string label, string text = "")
        {
            var result = ToggleTextFieldResult.Default;
            GUILayout.BeginHorizontal();
            result.Enabled = GUILayout.Toggle(toggled, label, GUILayout.ExpandWidth(false));
            GUILayout.Space(75);
            result.Text = GUILayout.TextField(text, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.EndHorizontal();
            return result;
        }

        private delegate void OnColorChangedAction(Color color);
        private void CustomColorButton(string name, Color initialColor, Color defaultColor, OnColorChangedAction onColorChangedAction)
        {
            var customColorButtonClicked = GUILayout.Button("Color", GUILayout.ExpandWidth(false));
            if (customColorButtonClicked)
            {
                LogDebug($"customColorButtonClicked {initialColor}");
                _colorPicker.OpenColorPicker(initialColor, defaultColor, $"Edit {name} color");
                _colorPicker.OnColorPickerClosed += color =>
                {
                    LogDebug($"color picker closed {initialColor} -> {color}");
                    if (color == initialColor) return;
                    onColorChangedAction.Invoke(color);
                };
            }
        }

        private delegate void Children();
        private void PoiContainer(POI poi, Children children)
        {
            GUILayout.BeginHorizontal();
            children.Invoke();
            GUILayout.FlexibleSpace();
            CustomColorButton(poi.Label, poi.Color, Settings.DefaultPoiColors[poi.Type], AssignPoiColorFactory(poi));
            GUILayout.EndHorizontal();
        }

        private OnColorChangedAction AssignPoiColorFactory(POI poi)
        {
            return color =>
            {
                LogDebug($"OnColorChangedAction body:{poi.Body.Serialize()}, type:{Enum.GetName(typeof(PoiType), poi.Type)}, color:{color.Serialize()}");
                poi.Color = color;
            };
        }

        private void DrawStandardPoi(CelestialBody body)
        {
            foreach (var poi in Settings.Instance.GetStandardPoisFor(body))
            {
                PoiContainer(poi, () =>
                    {
                        poi.Enabled = GUILayout.Toggle(poi.Enabled, poi.Label, GUILayout.ExpandWidth(false));
                        GUILayout.FlexibleSpace();
                    }
                );
            }
        }

        private void DrawPoiControls(CelestialBody body)
        {
            DrawStandardPoi(body);

            GUILayout.Space(10);

            foreach (var poi in Settings.Instance.GetCustomPoisFor(body))
            {
                CustomPoiGUI(poi);
            }

            if (GUILayout.Button("Add Custom POI"))
            {
                Settings.Instance.AddConfiguredPoi(POI.DefaultFrom(body, PoiType.Custom));
            }
        }

        private void DrawUI(int windowID)
        {
            GUILayout.BeginVertical();

                Controls.StandardCloseButton(CloseWindow, !Settings.Instance.UseTopRightCloseButton);

                Settings.Instance.GlobalEnable = GUILayout.Toggle(Settings.Instance.GlobalEnable, "Enabled");
                if (Settings.Instance.GlobalEnable)
                {
                    if (!OrbitPoiVisualizer.Instance.enabled)
                    {
                        LogDebug("[GUI] enabling OrbitPoiVisualizer");
                        OrbitPoiVisualizer.Instance.enabled = true;
                    }
                }

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                    Settings.Instance.FocusedBodyOnly = GUILayout.Toggle(Settings.Instance.FocusedBodyOnly, "Focused Body Only");
                    GUILayout.Label("(turning this off can have major performance impacts)");
                GUILayout.EndVertical();

                GUILayout.Space(10);

                Settings.Instance.EnableSpheres = GUILayout.Toggle(Settings.Instance.EnableSpheres, "Draw Spheres");
                Settings.Instance.AlignSpheres = GUILayout.Toggle(Settings.Instance.AlignSpheres, "Align Spheres");
                Settings.Instance.EnableCircles = GUILayout.Toggle(Settings.Instance.EnableCircles, "Draw Circles");

                Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies = GUILayout.Toggle(Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies, "Show POI Max Terrain Altitude On Atmospheric Bodies");
                
                GUILayout.Space(10);

                // TODO: make a dropdown
                _selectedBodyIndex = GUILayout.SelectionGrid(_selectedBodyIndex, _selectableBodyNames.ToArray(), 6);

                var selectedBodyName = _selectableBodyNames[_selectedBodyIndex];

                GUILayout.Space(10);

                var resetBodyPoiClicked = GUILayout.Button($"Reset POIs for {selectedBodyName} to defaults");
                if (resetBodyPoiClicked)
                {
                    LogDebug($"[GUI] Reset POIs for Body Clicked: {selectedBodyName}");
                    // TODO: it might be worth making a wrapper class for CelestialBodies
                    // so we can make null treated like a body with its own name
                    // this way we don't have to rely on all bodies having unique names
                    // or looping through them each time as CelestialBodyExtensions.ResolveByName adds some overhead
                    var targetBodyName = _selectedBodyIndex > 0 ? selectedBodyName : null;
                    var targetBody = CelestialBodyExtensions.ResolveByName(targetBodyName);
                    var configuredPoisToRemove = Settings.Instance.ConfiguredPois.Where(poi => poi.Type != PoiType.Custom && poi.Body == targetBody);

                    foreach (var poi in configuredPoisToRemove)
                    {
                        Settings.Instance.RemoveConfiguredPoi(poi);
                    }
                }

                GUILayout.Space(10);

                if (_selectedBodyIndex > 0)
                {
                    foreach (var body in FlightGlobals.Bodies)
                    {
                        if (selectedBodyName != body.name) continue;

                        DrawPoiControls(body);

                    }
                }
                else
                {
                    // global
                    DrawPoiControls(null);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Space(20);

                var showOptionsButtonClicked = GUILayout.Button("Show Options");
                if (showOptionsButtonClicked)
                {
                    LogDebug($"[GUI] Show Options Button clicked");
                    _optionsPopup.OpenOptionsPopup();
                }
            
            GUILayout.EndVertical();

            Controls.TopRightCloseButton(windowRect, CloseWindow, Settings.Instance.UseTopRightCloseButton);

            // // make only title bar be used for dragging
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 25));
        }

        private void CloseWindow()
        {
            LogDebug("CloseWindow");
            toolbarButton.SetFalse();
            showUI = false;
            _colorPicker.DisplayGUI(false);
            _optionsPopup.DisplayGUI(false);
        }

        void Update()
        {
            if (HighLogic.LoadedSceneHasPlanetarium || MapView.MapIsEnabled)
            {
                // UpdateWindowPos();
            }
        }

        private void OnDestroy()
        {
            LogDebug("[OnDestroy]");
            Cleanup();
        }
    }
}
