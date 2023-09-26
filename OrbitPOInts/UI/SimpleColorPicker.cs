using UnityEngine;
using UnityEngine.Events;

namespace OrbitPOInts.UI
{
    public class SimpleColorPicker : MonoBehaviour
    {
        private float _red = 1.0f;
        private float _green = 1.0f;
        private float _blue = 1.0f;
        private Color _color = Color.white;
        private Color _initialColor;

        private bool _showGUI;

        public UnityAction<Color> OnColorPickerClosed;

        public void OpenColorPicker(Color initialColor)
        {
            _initialColor = initialColor;
            _red = initialColor.r;
            _green = initialColor.g;
            _blue = initialColor.b;
            _showGUI = true;
        }

        public Color GetCurrentColor() => new Color(_red, _green, _blue);
        
        public void DisplayGUI(bool state)
        {
            _showGUI = state;
        }

        void OnGUI()
        {
            if (!_showGUI) return;

            GUILayout.BeginVertical();

            GUILayout.Label("Red");
            _red = GUILayout.HorizontalSlider(_red, 0.0f, 1.0f);

            GUILayout.Label("Green");
            _green = GUILayout.HorizontalSlider(_green, 0.0f, 1.0f);

            GUILayout.Label("Blue");
            _blue = GUILayout.HorizontalSlider(_blue, 0.0f, 1.0f);

            _color = new Color(_red, _green, _blue);

            // A simple way to display the color you've picked.
            GUIStyle colorBox = new GUIStyle();
            colorBox.normal.background = Texture2D.whiteTexture;
            GUI.color = _color;
            GUILayout.Box("", colorBox, GUILayout.Width(100), GUILayout.Height(100));
            GUI.color = Color.white;

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
                var cancelButtonClicked = GUILayout.Button("Cancel");
                var saveButtonClicked = GUILayout.Button("Save");
                if (cancelButtonClicked) CloseWindow(() => OnColorPickerClosed?.Invoke(_initialColor));
                if (saveButtonClicked) CloseWindow(() => OnColorPickerClosed?.Invoke(GetCurrentColor()));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private delegate void OnCloseAction();
        private void CloseWindow(OnCloseAction onCloseAction = null)
        {
            onCloseAction?.Invoke();
            DisplayGUI(false);
        }
    }
}
