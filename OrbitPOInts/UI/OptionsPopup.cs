#if TEST
using UnityEngineMock;
using UnityEngineMock.Events;
using KSP_HighLogic = KSPMock.HighLogic;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using UnityEngine.Events;
using KSP_HighLogic = HighLogic;
#endif
using OrbitPOInts.Data.POI;


namespace OrbitPOInts.UI
{
    using HighLogic = KSP_HighLogic;
    using Logger = Utils.Logger;

    // using instance and calling OnGUI()
    // [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class OptionsPopup : MonoBehaviour
    {

        private bool _useTopRightCloseButton = false;
        private bool _showGUI;
        
        private Rect _windowRect = new Rect(0, 0, 400, 200); // Initial size for the window

        public void OpenOptionsPopup()
        {
            LogDebug($"[OpenOptionsPopup]");
            DisplayGUI();
            CenterWindowPos();
        }

        private void LogDebug(string message)
        {
            Logger.LogDebug($"[OptionsUI] {message}");
        }

        private void Log(string message)
        {
            Logger.Log($"[OptionsUI] {message}");
        }
        
        public void DisplayGUI(bool state = true)
        {
            _showGUI = state;
        }

        internal void OnGUI()
        {
            if (!_showGUI) return;
            GUI.skin = Settings.Instance.UseSkin ? HighLogic.Skin : null;
            _windowRect = GUILayout.Window(1234567, _windowRect, DrawUI, "OrbitPOInts Options");
        }

        private void DrawUI(int windowID)
        {
            const int closeButtonSize = 25;
            GUILayout.BeginVertical();

                Controls.StandardCloseButton(CloseWindow, !Settings.Instance.UseTopRightCloseButton);

                GUILayout.Label("Misc");
                Settings.Instance.LogDebugEnabled = GUILayout.Toggle(Settings.Instance.LogDebugEnabled, "Enable Debug Level Logging");
                Settings.Instance.UseSkin = GUILayout.Toggle(Settings.Instance.UseSkin, "Use Skin");
                Settings.Instance.UseTopRightCloseButton = GUILayout.Toggle(Settings.Instance.UseTopRightCloseButton, "Use Top Right Close Button");

                GUILayout.Space(50);

                GUILayout.Label("-- Danger Zone -- (no confirmation and no undo)");
                var resetAllStandardPOIsButtonClicked = GUILayout.Button("Reset All Standard POIs to defaults");
                if (resetAllStandardPOIsButtonClicked)
                {
                    LogDebug("[GUI] Reset All Standard POIs to defaults clicked");
                    // we want to keep the custom pois
                    var customPois = Settings.Instance.ConfiguredPois.Where(poi => poi.Type == PoiType.Custom);
                    Settings.Instance.ClearConfiguredPois(customPois);
                }

                GUILayout.Space(10);

                var removeAllCustomPOIsButtonClicked = GUILayout.Button("Remove All Custom POIs");
                if (removeAllCustomPOIsButtonClicked)
                {
                    LogDebug("[GUI] Remove All Custom POIs clicked");
                    // we want to keep the standard pois
                    var standardPois = Settings.Instance.ConfiguredPois.Where(poi => poi.Type != PoiType.Custom);
                    Settings.Instance.ClearConfiguredPois(standardPois);
                }

            GUILayout.EndVertical();

            Controls.TopRightCloseButton(_windowRect, CloseWindow, Settings.Instance.UseTopRightCloseButton);

            // make only title bar be used for dragging
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 25));
        }

        private void CloseWindow()
        {
            LogDebug("CloseWindow");
            DisplayGUI(false);
        }

        private void CenterWindowPos()
        {
            if (_showGUI)
            {
                // Calculate the screen's center
                float centerX = Screen.width / 2;
                float centerY = Screen.height / 2;

                // Adjust for the window's size to get the top-left position
                _windowRect.x = centerX - (_windowRect.width / 2);
                _windowRect.y = centerY - (_windowRect.height / 2);
            }
        }
    }
}
