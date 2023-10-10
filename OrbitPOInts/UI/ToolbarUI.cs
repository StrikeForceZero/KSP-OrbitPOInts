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
    using CWIL = ControlWrapperInteractionLogger;

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
        private string _selectedBodyName;

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
                OnShowOrEnable,
                OnShowOrDisable,
                null,
                null,
                null,
                null,
                ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
                GameDatabase.Instance.GetTexture("OrbitPOInts/UI/toolbar_icon", false)
            );
            toolbarButton.onRightClick += OnRightClick;
            return true;
        }

        private bool RemoveToolbarButton()
        {
            if (!toolbarButton) return false;
            CloseWindow();
            ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
            return true;
        }

        private void ToggleWindow()
        {
            showUI = !showUI;
            CenterWindowPos();
            if (!showUI)
            {
                CloseWindow();
            }
        }

        private void OnShowOrEnable()
        {
            if (Settings.Instance.UseQuickEnableToggle)
            {
                Settings.Instance.GlobalEnable = true;
                return;
            }

            ToggleWindow();
        }

        private void OnShowOrDisable()
        {
            if (Settings.Instance.UseQuickEnableToggle)
            {
                Settings.Instance.GlobalEnable = false;
                return;
            }

            ToggleWindow();
        }

        private void OnRightClick()
        {
            ToggleWindow();
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
                windowRect = GUILayout.Window(12345, windowRect, DrawUI, "OrbitPOInts", WindowStyle.GetSharedDarkWindowStyle());
                _colorPicker.OnGUI();
                _optionsPopup.OnGUI();
            }
        }

        private static GUILayoutOption[] MergeOptions(GUILayoutOption[] optionsA, params GUILayoutOption[] optionsB) => optionsA.Concat(optionsB).ToArray();
        private static CWIL.RenderButton StandardButtonNoExpand(params GUILayoutOption[] options) => CWIL.StandardButton(MergeOptions(options, GUILayout.ExpandWidth(false)));
        private static CWIL.RenderToggle StandardToggleNoExpand(params GUILayoutOption[] options) => CWIL.StandardToggle(MergeOptions(options, GUILayout.ExpandWidth(false)));
        private static CWIL.RenderTextField StandardTextFieldNoExpand(params GUILayoutOption[] options) => CWIL.StandardTextField(MergeOptions(options, GUILayout.ExpandWidth(false)));

        private void LogContext()
        {
            LogDebug($"[Context:SelectedBodyName]{_selectedBodyName}");
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

            poi.AddPlanetRadius = CWIL.WrapToggle(poi.AddPlanetRadius, "+ PR", StandardToggleNoExpand(), _ => LogContext());
            GUILayout.Space(50);

            if (CWIL.WrapButton("Remove", StandardButtonNoExpand(), LogContext))
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
            var result = CWIL.WrapTextField(text, label);
            GUILayout.EndHorizontal();
            return result;
        }

        private ToggleTextFieldResult TextFieldWithToggle(bool toggled, string label, string text = "")
        {
            var result = ToggleTextFieldResult.Default;
            GUILayout.BeginHorizontal();
            result.Enabled = CWIL.WrapToggle(toggled, label, StandardToggleNoExpand(), _ => LogContext());
            GUILayout.Space(75);
            result.Text = CWIL.WrapTextField(text, label, StandardTextFieldNoExpand(GUILayout.Width(100)), _ => LogContext());
            GUILayout.EndHorizontal();
            return result;
        }

        private delegate void OnColorChangedAction(Color color);
        private void CustomColorButton(string name, Color initialColor, Color defaultColor, OnColorChangedAction onColorChangedAction)
        {
            CWIL.WrapButton(
                "Color",
                StandardButtonNoExpand(),
                () =>
                {
                    LogContext();
                    LogDebug($"customColorButtonClicked {initialColor}");
                    _colorPicker.OpenColorPicker(initialColor, defaultColor, $"Edit {name} color");
                    _colorPicker.OnColorPickerClosed += color =>
                    {
                        LogContext();
                        LogDebug($"color picker closed {initialColor} -> {color}");
                        if (color == initialColor) return;
                        onColorChangedAction.Invoke(color);
                    };
                }
            );
        }

        private delegate void Children();
        private void PoiContainer(POI poi, Children children)
        {
            GUILayout.BeginHorizontal();
            children.Invoke();
            GUILayout.FlexibleSpace();
            Controls.ColorBox(poi.Color);
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
                        poi.Enabled = CWIL.WrapToggle(poi.Enabled, poi.Label, StandardToggleNoExpand(), _ => LogContext());
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

            if (CWIL.WrapButton("Add Custom POI", null, LogContext))
            {
                Settings.Instance.AddConfiguredPoi(POI.DefaultFrom(body, PoiType.Custom));
            }
        }

        private void DrawUI(int windowID)
        {
            GUILayout.BeginVertical();

                Controls.StandardCloseButton(CloseWindow, !Settings.Instance.UseTopRightCloseButton);

                Settings.Instance.GlobalEnable = CWIL.WrapToggle(Settings.Instance.GlobalEnable,"Enabled");

                GUILayout.Space(10);

                CWIL.WrapButton("Force Refresh", StandardButtonNoExpand(), () =>
                {
                    GameStateManager.Instance.Visualizer.Rebuild();
                });

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                    Settings.Instance.FocusedBodyOnly = CWIL.WrapToggle(Settings.Instance.FocusedBodyOnly, "Focused Body Only");
                GUILayout.EndVertical();

                GUILayout.Space(10);

                Settings.Instance.EnableSpheres = CWIL.WrapToggle(Settings.Instance.EnableSpheres, "Draw Spheres");
                Settings.Instance.AlignSpheres = CWIL.WrapToggle(Settings.Instance.AlignSpheres, "Align Spheres");
                Settings.Instance.EnableCircles = CWIL.WrapToggle(Settings.Instance.EnableCircles, "Draw Circles");

                Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies = CWIL.WrapToggle(Settings.Instance.ShowPoiMaxTerrainAltitudeOnAtmosphericBodies, "Show POI Max Terrain Altitude On Atmospheric Bodies");
                
                GUILayout.Space(10);

                // TODO: make a dropdown
                _selectedBodyIndex = GUILayout.SelectionGrid(_selectedBodyIndex, _selectableBodyNames.ToArray(), 6);
                _selectedBodyName = _selectableBodyNames[_selectedBodyIndex];
                var selectedBody = _selectedBodyIndex > 0 ? FlightGlobals.Bodies[_selectedBodyIndex - 1] : null;

                GUILayout.Space(10);

                var resetBodyPoiClicked = CWIL.WrapButton($"Reset POIs for {_selectedBodyName} to defaults");
                if (resetBodyPoiClicked)
                {
                    LogDebug($"[GUI] Reset POIs for Body Clicked: {_selectedBodyName}");
                    var configuredPoisToRemove = Settings.Instance.ConfiguredPois.Where(poi => poi.Type != PoiType.Custom && poi.Body == selectedBody);

                    foreach (var poi in configuredPoisToRemove)
                    {
                        Settings.Instance.RemoveConfiguredPoi(poi);
                    }
                }

                GUILayout.Space(10);

                DrawPoiControls(selectedBody);

                GUILayout.FlexibleSpace();
                GUILayout.Space(20);

                var showOptionsButtonClicked = CWIL.WrapButton("Show Options");
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
            if (!Settings.Instance.UseQuickEnableToggle)
            {
                toolbarButton.SetFalse();
            }
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
