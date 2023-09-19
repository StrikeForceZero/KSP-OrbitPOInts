using KSP.UI.Screens;
using UnityEngine;

namespace OrbitPOInts
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ToolbarUI : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private bool showUI = false;

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
        }

        private void OnGUI()
        {
            if (showUI)
            {
                GUI.skin = HighLogic.Skin;
                GUILayout.BeginVertical();
                GUILayout.Label("Hello, KSP!");
                GUILayout.EndVertical();
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