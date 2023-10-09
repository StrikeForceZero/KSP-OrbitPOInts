using System;
using Logger = OrbitPOInts.Utils.Logger;
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
    using CWIL = ControlWrapperInteractionLogger;

    public static class Controls
    {
        const int closeButtonSize = 25;
        private const string closeButtonLabel = "X";

        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[Controls] {message}");
        }

        private static void Log(string message)
        {
            Logger.Log($"[Controls] {message}");
        }

        public static void StandardCloseButton(Action onCloseButtonClick, bool show = true)
        {
            if (!show) return;

            GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace(); // Pushes the following items to the right

                CWIL.WrapButton(
                    closeButtonLabel,
                    CWIL.StandardButton(GUILayout.Width(closeButtonSize)),
                    () =>
                    {
                        LogDebug("[StandardCloseButton][GUI] StandardCloseButton normal click");
                        onCloseButtonClick.Invoke();
                    }
                );


            GUILayout.EndHorizontal();
        }

        public static void TopRightCloseButton(Rect windowRect, Action onCloseButtonClick, bool show = true)
        {
            if (!show) return;

            const float padding = 5; // Padding from the edge of the window
            var closeButtonRect = new Rect(windowRect.width - closeButtonSize - padding, padding, closeButtonSize, closeButtonSize);
            CWIL.WrapButton(
                closeButtonLabel,
                label => GUI.Button(closeButtonRect, label),
                () =>
                {
                    LogDebug("[TopRightCloseButton][GUI] TopRightCloseButton normal click");
                    onCloseButtonClick.Invoke();
                }
            );

            // GUI.Button never captures the mouse when rendered on top of GUILayout
            if (Event.current.type != EventType.MouseDown) return;
            if (!closeButtonRect.Contains(Event.current.mousePosition)) return;

            LogDebug("[TopRightCloseButton][GUI] TopRightCloseButton intercepted click");

            onCloseButtonClick.Invoke();
            Event.current.Use();
        }

        public static void ColorBox(Color color, uint width = 25, uint height = 25)
        {
            var colorBox = new GUIStyle
            {
                normal =
                {
                    background = Texture2D.whiteTexture
                }
            };
            GUI.color = color;
            GUILayout.Box("", colorBox, GUILayout.Width(width), GUILayout.Height(height));
            GUI.color = Color.white;
        }
    }
}
