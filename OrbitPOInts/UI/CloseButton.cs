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
    public static class CloseButton
    {
        const int closeButtonSize = 25;
        private const string Label = "X";

        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[CloseButton] {message}");
        }

        private static void Log(string message)
        {
            Logger.Log($"[CloseButton] {message}");
        }

        public static void StandardCloseButton(Action onCloseButtonClick)
        {
            if (Settings.Instance.UseTopRightCloseButton) return;

            GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace(); // Pushes the following items to the right

                if (GUILayout.Button(Label, GUILayout.Width(closeButtonSize)))
                {
                    LogDebug("[GUI] StandardCloseButton normal click");
                    onCloseButtonClick.Invoke();
                }

            GUILayout.EndHorizontal();
        }

        public static void TopRightCloseButton(Rect windowRect, Action onCloseButtonClick)
        {
            if (!Settings.Instance.UseTopRightCloseButton) return;

            const float padding = 5; // Padding from the edge of the window
            var closeButtonRect = new Rect(windowRect.width - closeButtonSize - padding, padding, closeButtonSize, closeButtonSize);
            var closeButtonClicked = GUI.Button(closeButtonRect, Label);

            if (closeButtonClicked)
            {
                LogDebug("[GUI] TopRightCloseButton normal click");
                onCloseButtonClick.Invoke();
                return;
            }

            // GUI.Button never captures the mouse when rendered on top of GUILayout
            if (Event.current.type != EventType.MouseDown) return;
            if (!closeButtonRect.Contains(Event.current.mousePosition)) return;

            LogDebug("[GUI] TopRightCloseButton intercepted click");

            onCloseButtonClick.Invoke();
            Event.current.Use();
        }
    }
}
