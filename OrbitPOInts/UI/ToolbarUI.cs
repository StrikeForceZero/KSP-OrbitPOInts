using System;
using System.Globalization;
using KSP.UI.Screens;
using OrbitPOInts.Extensions;
using OrbitPOInts.UI;
using UnityEngine;

namespace OrbitPOInts
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ToolbarUI : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private bool showUI = false;
        private Rect windowRect = new Rect(0, 0, 400, 200); // Initial size for the window
        private bool _useTopRightCloseButton = false;
        private bool _eventsRegistered;

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
            }
        }

        private delegate double GetCustomPoiRadiusAction();
        private delegate void SetCustomPoiRadiusAction(double radius);

        private delegate bool GetCustomPoiEnabledAction();
        private delegate void SetCustomPoiEnabledAction(bool enabled);

        // TODO: needs moar abstraction!
        private void CustomPoiHandler(
            GetCustomPoiRadiusAction getCustomPoiRadiusAction,
            SetCustomPoiRadiusAction setCustomPoiRadiusAction,
            GetCustomPoiEnabledAction getCustomPoiEnabledAction,
            SetCustomPoiEnabledAction setCustomPoiEnabledAction,
            string label
        )
        {
            // save the old values for checking later
            var oldPoi = getCustomPoiRadiusAction.Invoke();
            var customPoiInput = TextFieldWithToggle(getCustomPoiEnabledAction.Invoke(), label, oldPoi.ToString("N", CultureInfo.CurrentCulture));
            var result1 = double.TryParse(customPoiInput.Text, out var newPoi);
            // ensure value is always positive
            if (result1) setCustomPoiRadiusAction(Math.Abs(newPoi));
            newPoi = getCustomPoiRadiusAction.Invoke();
            // check if the values were changed
            var poiChanged = !newPoi.AreRelativelyEqual(oldPoi);
            setCustomPoiEnabledAction(newPoi > 0 && (poiChanged || customPoiInput.Enabled));
            customPoiInput.Text = newPoi.ToString("N", CultureInfo.CurrentCulture);
        }

        private void CustomPoiGUI()
        {
            GUILayout.Space(10);
            Settings.CustomPOiFromCenter = GUILayout.Toggle(Settings.CustomPOiFromCenter, "Draw Custom POIs from body center");
            GUILayout.Space(10);

            PoiContainer(
                PoiName.Custom1,
                () =>
                {
                    CustomPoiHandler(
                        () => Settings.CustomPOI1,
                        radius => Settings.CustomPOI1 = radius,
                        () => Settings.CustomPOI1Enabled,
                        enabled => Settings.CustomPOI1Enabled = enabled,
                        "Custom POI 1:"
                    );
                },
                Settings.PoiColors[PoiName.Custom1],
                AssignPoiColorFactory(PoiName.Custom1)
            );

            PoiContainer(
                PoiName.Custom2,
                () =>
                {
                    CustomPoiHandler(
                        () => Settings.CustomPOI2,
                        radius => Settings.CustomPOI2 = radius,
                        () => Settings.CustomPOI2Enabled,
                        enabled => Settings.CustomPOI2Enabled = enabled,
                        "Custom POI 2:"
                    );
                },
                Settings.PoiColors[PoiName.Custom2],
                AssignPoiColorFactory(PoiName.Custom2)
            );

            PoiContainer(
                PoiName.Custom3,
                () =>
                {
                    CustomPoiHandler(
                        () => Settings.CustomPOI3,
                        radius => Settings.CustomPOI3 = radius,
                        () => Settings.CustomPOI3Enabled,
                        enabled => Settings.CustomPOI3Enabled = enabled,
                        "Custom POI 3:"
                    );
                },
                Settings.PoiColors[PoiName.Custom3],
                AssignPoiColorFactory(PoiName.Custom3)
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
        private void CustomColorButton(PoiName name, Color initialColor, OnColorChangedAction onColorChangedAction)
        {
            var customColorButtonClicked = GUILayout.Button("Color", GUILayout.ExpandWidth(false));
            if (customColorButtonClicked)
            {
                LogDebug($"customColorButtonClicked {initialColor}");
                _colorPicker.OpenColorPicker(initialColor, $"Edit {Enum.GetName(typeof(PoiName), name)} color");
                _colorPicker.OnColorPickerClosed += onColorChangedAction.Invoke;
            }
        }

        private delegate void Children();
        private void PoiContainer(PoiName name, Children children, Color initialColor, OnColorChangedAction onColorChangedAction)
        {
            GUILayout.BeginHorizontal();
            children.Invoke();
            GUILayout.FlexibleSpace();
            CustomColorButton(name, initialColor, onColorChangedAction);
            GUILayout.EndHorizontal();
        }

        private OnColorChangedAction AssignPoiColorFactory(PoiName name)
        {
            return color =>
            {
                LogDebug($"OnColorChangedAction {Enum.GetName(typeof(PoiName), name)}: {color}");
                Settings.PoiColors[name] = color;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            };
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

                Settings.GlobalEnable = GUILayout.Toggle(Settings.GlobalEnable, "Enabled");

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                    Settings.ActiveBodyOnly = GUILayout.Toggle(Settings.ActiveBodyOnly, "Active Body Only");
                    GUILayout.Label("(turning this off can have major performance impacts)");
                GUILayout.EndVertical();

                GUILayout.Space(10);

                Settings.EnableSpheres = GUILayout.Toggle(Settings.EnableSpheres, "Draw Spheres");
                Settings.AlignSpheres = GUILayout.Toggle(Settings.AlignSpheres, "Align Spheres");
                Settings.EnableCircles = GUILayout.Toggle(Settings.EnableCircles, "Draw Circles");
                
                GUILayout.Space(10);

                // TODO: abstract even more
                PoiContainer(
                    PoiName.HillSphere,
                    () => { Settings.EnablePOI_HillSphere = GUILayout.Toggle(Settings.EnablePOI_HillSphere, "POI HillSphere", GUILayout.ExpandWidth(false)); GUILayout.FlexibleSpace(); },
                    Settings.PoiColors[PoiName.HillSphere],
                    AssignPoiColorFactory(PoiName.HillSphere)
                );
                PoiContainer(
                    PoiName.SOI,
                    () => { Settings.EnablePOI_SOI = GUILayout.Toggle(Settings.EnablePOI_SOI, "POI SOI", GUILayout.ExpandWidth(false)); GUILayout.FlexibleSpace(); },
                    Settings.PoiColors[PoiName.SOI],
                    AssignPoiColorFactory(PoiName.SOI)
                );
                PoiContainer(
                    PoiName.Atmo,
                    () => { Settings.EnablePOI_Atmo = GUILayout.Toggle(Settings.EnablePOI_Atmo, "POI Atmosphere", GUILayout.ExpandWidth(false)); GUILayout.FlexibleSpace(); },
                    Settings.PoiColors[PoiName.Atmo],
                    AssignPoiColorFactory(PoiName.Atmo)
                );
                PoiContainer(
                    PoiName.MinOrbit,
                    () => { Settings.EnablePOI_MinOrbit = GUILayout.Toggle(Settings.EnablePOI_MinOrbit, "POI Minimum Orbit", GUILayout.ExpandWidth(false)); GUILayout.FlexibleSpace(); },
                    Settings.PoiColors[PoiName.MinOrbit],
                    AssignPoiColorFactory(PoiName.MinOrbit)
                );
                PoiContainer(
                    PoiName.MaxAlt,
                    () => { Settings.EnablePOI_MaxAlt = GUILayout.Toggle(Settings.EnablePOI_MaxAlt, "POI MaxAlt", GUILayout.ExpandWidth(false)); GUILayout.FlexibleSpace(); },
                    Settings.PoiColors[PoiName.MaxAlt],
                    AssignPoiColorFactory(PoiName.MaxAlt)
                );
                
                    GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        Settings.ShowPOI_MaxAlt_OnAtmoBodies = GUILayout.Toggle(Settings.ShowPOI_MaxAlt_OnAtmoBodies,
                            "Show POI Max Altitude On Atmosphere Bodies");
                    GUILayout.EndHorizontal();

                CustomPoiGUI();

                GUILayout.FlexibleSpace();

                Settings.LogDebugEnabled = GUILayout.Toggle(Settings.LogDebugEnabled, "Enable Debug Level Logging");
            
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
