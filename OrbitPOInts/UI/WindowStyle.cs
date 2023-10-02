#if TEST
using UnityEngineMock;
using UnityEngineMock.Events;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using UnityEngine.Events;
#endif

namespace OrbitPOInts.UI
{
    public static class WindowStyle
    {
        private static readonly GUIStyle _windowStyle = DefaultStyle;
        private static readonly Color _opaqueBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 2f); // Dark and Opaque
        private static Texture2D _opaqueBackgroundTexture => MakeTexture(2, 2, _opaqueBackgroundColor);

        public static GUIStyle DefaultStyle => new GUIStyle(GUI.skin.window);

        public static void ApplyDefaults()
        {
            var defaultStyle = DefaultStyle;
            _windowStyle.normal.background = defaultStyle.normal.background;
            _windowStyle.active.background = defaultStyle.active.background;
            _windowStyle.focused.background = defaultStyle.focused.background;
            _windowStyle.hover.background = defaultStyle.hover.background;
            _windowStyle.onNormal.background = defaultStyle.onNormal.background;
            _windowStyle.onActive.background = defaultStyle.onActive.background;
            _windowStyle.onFocused.background = defaultStyle.onFocused.background;
            _windowStyle.onHover.background = defaultStyle.onHover.background;
        }

        public static void ApplyOverride()
        {
            _windowStyle.normal.background = _opaqueBackgroundTexture;
            //_windowStyle.active.background = _opaqueBackgroundTexture;
            //_windowStyle.focused.background = _opaqueBackgroundTexture;
            //_windowStyle.hover.background = _opaqueBackgroundTexture;
            _windowStyle.onNormal.background = _opaqueBackgroundTexture;
            //_windowStyle.onActive.background = _opaqueBackgroundTexture;
            //_windowStyle.onFocused.background = _opaqueBackgroundTexture;
            //_windowStyle.onHover.background = _opaqueBackgroundTexture;
        }

        public static GUIStyle GetSharedDarkWindowStyle()
        {
            if (Settings.Instance.UseSkin || !Settings.Instance.UseOpaqueBackgroundOverride)
            {
                ApplyDefaults();
            }
            else
            {
                ApplyOverride();
            }
            return _windowStyle;
        }

        private static Texture2D MakeTexture(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
                pix[i] = col;

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}
