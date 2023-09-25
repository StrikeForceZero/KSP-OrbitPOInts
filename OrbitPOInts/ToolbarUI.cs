using System;
using System.Globalization;
using KSP.UI.Screens;
using OrbitPOInts.Extensions;
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

        private void Log(string message)
        {
            Logger.Log($"[ToolbarUI] {message}");
        }

        private void Awake()
        {
            Log("[Awake]");
            RegisterEvents();
            FixState();
        }

        private void Start()
        {
            Log("[Start]");
            RegisterEvents();
            FixState();
        }

        private void OnEnable()
        {
            Log("[OnEnable]");
            RegisterEvents();
            FixState();
        }

        private void OnDisable()
        {
            Log("[OnDisable]");
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
                Log("RegisterEvents");
                GameEvents.OnMapEntered.Add(OnMapEntered);
                GameEvents.OnMapExited.Add(OnMapExited);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
                _eventsRegistered = true;
                return;
            }

            if (!_eventsRegistered) return;
            Log("UnRegisterEvents");
            GameEvents.OnMapEntered.Remove(OnMapEntered);
            GameEvents.OnMapExited.Remove(OnMapExited);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            _eventsRegistered = false;
        }

        private void FixState()
        {
            Log($"[FixState] ViewingMapOrTrackingStation: {Lib.ViewingMapOrTrackingStation} scene: {Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene)}");
            if (!Lib.ViewingMapOrTrackingStation)
            {
                if (RemoveToolbarButton())
                {
                    Log("[FixState] Remove button");
                }
                return;
            }

            if (CreateToolbarButton())
            {
                Log("[FixState] Add button");
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
            Log("[OnMapEntered]");
            FixState();
        }

        private void OnMapExited()
        {
            Log("[OnMapExited]");
            FixState();
        }

        private void OnGameSceneLoadRequested(GameScenes scenes)
        {
            Log($"[OnGameSceneLoadRequested] {Enum.GetName(typeof(GameScenes), scenes)}");
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

        private void CustomPoiGUI()
        {
            // save the old values for checking later
            var oldPoi1 = Settings.CustomPOI1;
            var oldPoi2 = Settings.CustomPOI2;
            var oldPoi3 = Settings.CustomPOI3;

            var customPoi1Input = TextFieldWithToggle(Settings.CustomPOI1Enabled, "Custom POI 1: ", Settings.CustomPOI1.ToString("N", CultureInfo.CurrentCulture));
            var customPoi2Input = TextFieldWithToggle(Settings.CustomPOI2Enabled, "Custom POI 2: ", Settings.CustomPOI2.ToString("N", CultureInfo.CurrentCulture));
            var customPoi3Input = TextFieldWithToggle(Settings.CustomPOI3Enabled, "Custom POI 3: ", Settings.CustomPOI3.ToString("N", CultureInfo.CurrentCulture));
            var result1 = double.TryParse(customPoi1Input.Text, out var customPoi1);
            var result2 = double.TryParse(customPoi2Input.Text, out var customPoi2);
            var result3 = double.TryParse(customPoi3Input.Text, out var customPoi3);
            // ensure value is always positive
            if (result1) Settings.CustomPOI1 = Math.Abs(customPoi1);
            if (result2) Settings.CustomPOI2 = Math.Abs(customPoi2);
            if (result3) Settings.CustomPOI3 = Math.Abs(customPoi3);

            // check if the values were changed
            var poi1Changed = !Settings.CustomPOI1.AreRelativelyEqual(oldPoi1);
            var poi2Changed = !Settings.CustomPOI2.AreRelativelyEqual(oldPoi2);
            var poi3Changed = !Settings.CustomPOI3.AreRelativelyEqual(oldPoi3);

            // if the value was updated and >0 enable it, otherwise disable it
            Settings.CustomPOI1Enabled = Settings.CustomPOI1 > 0 && (poi1Changed || customPoi1Input.Enabled);
            Settings.CustomPOI2Enabled = Settings.CustomPOI2 > 0 && (poi2Changed || customPoi2Input.Enabled);
            Settings.CustomPOI3Enabled = Settings.CustomPOI3 > 0 && (poi3Changed || customPoi3Input.Enabled);

            // ReSharper disable RedundantAssignment
            customPoi1Input.Text = Settings.CustomPOI1.ToString("N", CultureInfo.CurrentCulture);
            customPoi2Input.Text = Settings.CustomPOI2.ToString("N", CultureInfo.CurrentCulture);
            customPoi3Input.Text = Settings.CustomPOI3.ToString("N", CultureInfo.CurrentCulture);
            // ReSharper restore RedundantAssignment
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
            result.Enabled = GUILayout.Toggle(toggled, label);
            // TODO: why is the toggle label getting cut off?
            GUILayout.Space(100);
            result.Text = GUILayout.TextField(text, GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.EndHorizontal();
            return result;
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
                            Log("[GUI] Close button clicked");
                            CloseWindow();
                        }

                    GUILayout.EndHorizontal();
                }

                Settings.GlobalEnable = GUILayout.Toggle(Settings.GlobalEnable, "Enabled");

                GUILayout.Space(10);

                Settings.EnableSpheres = GUILayout.Toggle(Settings.EnableSpheres, "Draw Spheres");
                Settings.AlignSpheres = GUILayout.Toggle(Settings.AlignSpheres, "Align Spheres");
                Settings.EnableCircles = GUILayout.Toggle(Settings.EnableCircles, "Draw Circles");
                
                GUILayout.Space(10);

                Settings.EnablePOI_HillSphere = GUILayout.Toggle(Settings.EnablePOI_HillSphere, "POI HillSphere");
                Settings.EnablePOI_SOI = GUILayout.Toggle(Settings.EnablePOI_SOI, "POI SOI");
                Settings.EnablePOI_Atmo = GUILayout.Toggle(Settings.EnablePOI_Atmo, "POI Atmosphere");
                Settings.EnablePOI_MinOrbit = GUILayout.Toggle(Settings.EnablePOI_MinOrbit, "POI Minimum Orbit");
                Settings.EnablePOI_MaxAlt = GUILayout.Toggle(Settings.EnablePOI_MaxAlt, "POI MaxAlt");
                
                    GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        Settings.ShowPOI_MaxAlt_OnAtmoBodies = GUILayout.Toggle(Settings.ShowPOI_MaxAlt_OnAtmoBodies,
                            "Show POI Max Altitude On Atmosphere Bodies");
                    GUILayout.EndHorizontal();

                CustomPoiGUI();
            
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
                        Log("[GUI] Close button clicked");
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
            Log("[OnDestroy]");
            Cleanup();
        }
    }

    sealed class ToggleTextFieldResult
    {
        public bool Enabled;
        public string Text;

        public static ToggleTextFieldResult Default => new ToggleTextFieldResult();
    }
}
