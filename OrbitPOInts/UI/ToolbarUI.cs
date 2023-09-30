using System;
using System.Collections.Generic;
using System.Globalization;
using OrbitPOInts.Data;
using OrbitPOInts.Extensions;

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

            LogDebug($"[FixState] ViewingMapOrTrackingStation: {Lib.ViewingMapOrTrackingStation} scene: {Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene)}");
            if (!Lib.ViewingMapOrTrackingStation)
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
            LogDebug($"[OnGameSceneLoadRequested] {Lib.GetSceneName(scenes)}");
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
                GUI.skin = HighLogic.Skin;
                windowRect = GUILayout.Window(12345, windowRect, DrawUI, "OrbitPOInts");
                _colorPicker.OnGUI();
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

            // TODO: handle POI.AddPlanetRadius

            // TODO: add remove button
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
            GUILayout.Space(100);
            result.Text = GUILayout.TextField(text, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.EndHorizontal();
            return result;
        }

        private delegate void OnColorChangedAction(Color color);
        private void CustomColorButton(string name, Color initialColor, OnColorChangedAction onColorChangedAction)
        {
            var customColorButtonClicked = GUILayout.Button("Color", GUILayout.ExpandWidth(false));
            if (customColorButtonClicked)
            {
                LogDebug($"customColorButtonClicked {initialColor}");
                _colorPicker.OpenColorPicker(initialColor, $"Edit {name} color");
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
            CustomColorButton(poi.Label, poi.Color, AssignPoiColorFactory(poi));
            GUILayout.EndHorizontal();
        }

        private OnColorChangedAction AssignPoiColorFactory(POI poi)
        {
            return color =>
            {
                LogDebug($"OnColorChangedAction body:{poi.Body.Serialize()}, type:{Enum.GetName(typeof(PoiType), poi.Type)}, color:{color.Serialize()}");
                poi.Color = color;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
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

            if (GUILayout.Button("+"))
            {
                Settings.Instance.AddPoi(POI.DefaultFrom(body, PoiType.Custom));
            }
        }

        private void DrawUI(int windowID)
        {
            const int closeButtonSize = 25;
            GUILayout.BeginVertical();

                if (!_useTopRightCloseButton) {
                    GUILayout.BeginHorizontal();

                        GUILayout.FlexibleSpace(); // Pushes the following items to the right

                        if (GUILayout.Button("X", GUILayout.Width(closeButtonSize)))
                        {
                            LogDebug("[GUI] Close button clicked");
                            CloseWindow();
                        }

                    GUILayout.EndHorizontal();
                }

                Settings.Instance.GlobalEnable = GUILayout.Toggle(Settings.Instance.GlobalEnable, "Enabled");

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

                if (_selectedBodyIndex > 0)
                {
                    foreach (var body in FlightGlobals.Bodies)
                    {
                        if (_selectableBodyNames[_selectedBodyIndex] != body.name) continue;

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

                Settings.Instance.LogDebugEnabled = GUILayout.Toggle(Settings.Instance.LogDebugEnabled, "Enable Debug Level Logging");
            
            GUILayout.EndVertical();

            // Make the window draggable
            GUI.DragWindow();

            // TODO: the GUILayout close button seems out of place
            if (_useTopRightCloseButton)
            {
                const float padding = 5; // Padding from the edge of the window
                var closeButtonRect = new Rect(windowRect.width - closeButtonSize - padding, padding, closeButtonSize, closeButtonSize);
                GUI.Button(closeButtonRect, "X");

                // GUI.Button never captures the mouse when rendered on top of GUILayout
                if (Event.current.type == EventType.MouseDown)
                {
                    if (closeButtonRect.Contains(Event.current.mousePosition))
                    {
                        LogDebug("[GUI] Close button clicked");
                        CloseWindow();
                        Event.current.Use();
                        return;
                    }
                }
            }
        }

        private void CloseWindow()
        {
            toolbarButton.SetFalse();
            showUI = false;
            _colorPicker.DisplayGUI(false);
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
