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

                Settings.Instance.LogDebugEnabled = GUILayout.Toggle(Settings.Instance.LogDebugEnabled, "Enable Debug Level Logging");
                Settings.Instance.UseSkin = GUILayout.Toggle(Settings.Instance.UseSkin, "Use Skin");

            GUILayout.EndVertical();

            GUI.DragWindow();

            // TODO: the GUILayout close button seems out of place
            if (_useTopRightCloseButton)
            {
                const float padding = 5; // Padding from the edge of the window
                var closeButtonRect = new Rect(_windowRect.width - closeButtonSize - padding, padding, closeButtonSize, closeButtonSize);
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
