using KSP.UI.Screens;
using UnityEngine;

namespace OrbitPOInts
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ToolbarUI : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private bool showUI = false;
        private Rect windowRect = new Rect(0, 0, 250, 100); // Initial size for the window

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

        private void DrawUI(int windowID)
        {
            GUILayout.BeginVertical();
            Settings.GlobalEnable = GUILayout.Toggle(Settings.GlobalEnable, "Enabled");
            Settings.EnableSpheres = GUILayout.Toggle(Settings.EnableSpheres, "Draw Spheres");
            Settings.AlignSpheres = GUILayout.Toggle(Settings.AlignSpheres, "Align Spheres");
            Settings.EnableCircles = GUILayout.Toggle(Settings.EnableCircles, "Draw Circles");
            Settings.EnablePOI_HillSphere = GUILayout.Toggle(Settings.EnablePOI_HillSphere, "POI HillSphere");
            Settings.EnablePOI_SOI = GUILayout.Toggle(Settings.EnablePOI_SOI, "POI SOI");
            Settings.EnablePOI_Atmo = GUILayout.Toggle(Settings.EnablePOI_Atmo, "POI Atmosphere");
            Settings.EnablePOI_MinOrbit = GUILayout.Toggle(Settings.EnablePOI_MinOrbit, "POI Minimum Orbit");
            Settings.EnablePOI_MaxAlt = GUILayout.Toggle(Settings.EnablePOI_MaxAlt, "POI MaxAlt");
            Settings.ShowPOI_MaxAlt_OnAtmoBodies = GUILayout.Toggle(Settings.ShowPOI_MaxAlt_OnAtmoBodies, "Show POI Max Altitude On Atmosphere Bodies");
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
}