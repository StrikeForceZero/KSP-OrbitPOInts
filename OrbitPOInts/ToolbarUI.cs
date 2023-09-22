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

        private void Start()
        {
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
        }

        private void OnToolbarButtonClick()
        {
            showUI = !showUI;
            CenterWindowPos();
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

            var customPoi1Input = TextFieldWithToggle(Settings.CustomPOI1Enabled, "Custom POI 1: ", Settings.CustomPOI1.ToString(CultureInfo.CurrentCulture));
            var customPoi2Input = TextFieldWithToggle(Settings.CustomPOI2Enabled, "Custom POI 2: ", Settings.CustomPOI2.ToString(CultureInfo.CurrentCulture));
            var customPoi3Input = TextFieldWithToggle(Settings.CustomPOI3Enabled, "Custom POI 3: ", Settings.CustomPOI3.ToString(CultureInfo.CurrentCulture));
            var result1 = double.TryParse(customPoi1Input.Text, out var customPoi1);
            var result2 = double.TryParse(customPoi2Input.Text, out var customPoi2);
            var result3 = double.TryParse(customPoi3Input.Text, out var customPoi3);
            if (result1) Settings.CustomPOI1 = Math.Max(0, customPoi1);
            if (result2) Settings.CustomPOI2 = Math.Max(0, customPoi2);
            if (result3) Settings.CustomPOI3 = Math.Max(0, customPoi3);

            // check if the values were changed
            var poi1Changed = !Settings.CustomPOI1.AreRelativelyEqual(oldPoi1);
            var poi2Changed = !Settings.CustomPOI2.AreRelativelyEqual(oldPoi2);
            var poi3Changed = !Settings.CustomPOI3.AreRelativelyEqual(oldPoi3);

            // if the value was updated and >0 enable it, otherwise disable it
            Settings.CustomPOI1Enabled = Settings.CustomPOI1 > 0 && (poi1Changed || customPoi1Input.Enabled);
            Settings.CustomPOI2Enabled = Settings.CustomPOI2 > 0 && (poi2Changed || customPoi2Input.Enabled);
            Settings.CustomPOI3Enabled = Settings.CustomPOI3 > 0 && (poi3Changed || customPoi3Input.Enabled);

            // ReSharper disable RedundantAssignment
            customPoi1Input.Text = Settings.CustomPOI1.ToString(CultureInfo.CurrentCulture);
            customPoi2Input.Text = Settings.CustomPOI2.ToString(CultureInfo.CurrentCulture);
            customPoi3Input.Text = Settings.CustomPOI3.ToString(CultureInfo.CurrentCulture);
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
            result.Text = GUILayout.TextField(text);
            GUILayout.EndHorizontal();
            return result;
        }

        private void DrawUI(int windowID)
        {
            GUILayout.BeginVertical();
            
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
            if (toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
            }
        }
    }

    sealed class ToggleTextFieldResult
    {
        public bool Enabled;
        public string Text;

        public static ToggleTextFieldResult Default => new ToggleTextFieldResult();
    }
}
