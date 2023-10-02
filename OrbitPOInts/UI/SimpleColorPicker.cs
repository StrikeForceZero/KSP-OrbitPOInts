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
    public class SimpleColorPicker : MonoBehaviour
    {
        private float _red = 1.0f;
        private float _green = 1.0f;
        private float _blue = 1.0f;
        private Color _color = Color.white;
        private Color _initialColor;
        private Color _defaultColor;
        private string _title;

        private bool _showGUI;
        
        private Rect _windowRect = new Rect(0, 0, 400, 200); // Initial size for the window

        public UnityAction<Color> OnColorPickerClosed;

        public void OpenColorPicker(Color initialColor, Color defaultColor, string title)
        {
            LogDebug($"[OpenColorPicker]");
            _title = title;
            _initialColor = initialColor;
            _defaultColor = defaultColor;
            _red = initialColor.r;
            _green = initialColor.g;
            _blue = initialColor.b;
            DisplayGUI();
            CenterWindowPos();
        }

        private void LogDebug(string message)
        {
            Logger.LogDebug($"[SimpleColorPicker] {message}");
        }

        private void Log(string message)
        {
            Logger.Log($"[SimpleColorPicker] {message}");
        }

        public Color GetCurrentColor() => new Color(_red, _green, _blue);
        
        public void DisplayGUI(bool state = true)
        {
            _showGUI = state;
        }

        internal void OnGUI()
        {
            if (!_showGUI) return;
            GUI.skin = Settings.Instance.UseSkin ? HighLogic.Skin : null;
            _windowRect = GUILayout.Window(123456, _windowRect, DrawUI, _title);
        }

        private void DrawUI(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Red");
            _red = GUILayout.HorizontalSlider(_red, 0.0f, 1.0f);

            GUILayout.Label("Green");
            _green = GUILayout.HorizontalSlider(_green, 0.0f, 1.0f);

            GUILayout.Label("Blue");
            _blue = GUILayout.HorizontalSlider(_blue, 0.0f, 1.0f);

            _color = new Color(_red, _green, _blue);

            // A simple way to display the color you've picked.
            Controls.ColorBox(_color, 100, 100);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
                var cancelButtonClicked = GUILayout.Button("Cancel");
                var defaultButtonClicked = GUILayout.Button("Default");
                var saveButtonClicked = GUILayout.Button("Save");
                if (cancelButtonClicked) CloseWindow(() => OnColorPickerClosed?.Invoke(_initialColor));
                if (defaultButtonClicked) CloseWindow(() => OnColorPickerClosed?.Invoke(_defaultColor));
                if (saveButtonClicked) CloseWindow(() => OnColorPickerClosed?.Invoke(GetCurrentColor()));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private delegate void OnCloseAction();
        private void CloseWindow(OnCloseAction onCloseAction = null)
        {
            LogDebug("[CloseWindow]");
            onCloseAction?.Invoke();
            OnColorPickerClosed = null;
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
