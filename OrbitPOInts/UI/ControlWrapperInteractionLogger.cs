using System;
using Logger = OrbitPOInts.Utils.Logger;

#if TEST
using System.Linq;
using UnityEngineMock;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.UI
{
    public static class ControlWrapperInteractionLogger
    {
        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[ControlWrapperInteractionLogger] {message}");
        }

        public delegate bool RenderToggle(bool initialValue, string label);
        public delegate void OnToggleOnChange(bool result);
        public delegate bool RenderButton(string label);
        public delegate void OnButtonOnClick();
        public delegate string RenderTextField(string initialValue, string label);
        public delegate void OnTextFieldOnChange(string result);


        public static RenderButton StandardButton(params GUILayoutOption[] options) => (label) => GUILayout.Button(label, options);
        public static RenderToggle StandardToggle(params GUILayoutOption[] options) => (initialValue, label) => GUILayout.Toggle(initialValue, label, options);
        public static RenderTextField StandardTextField(params GUILayoutOption[] options) => (initialValue, label) => GUILayout.TextField(initialValue, options);

        public static bool WrapToggle(bool initialValue, string label, RenderToggle renderControl = null, OnToggleOnChange onChange = null)
        {
            renderControl ??= StandardToggle();
            var result = renderControl(initialValue, label);
            if (result != initialValue)
            {
                LogDebug($"[Toggle]['{label}'] {initialValue} -> {result}");
                onChange?.Invoke(result);
            }
            return result;
        }

        public static bool WrapButton(string label, RenderButton renderControl = null, OnButtonOnClick onClick = null)
        {
            renderControl ??= StandardButton();
            var clicked = renderControl(label);
            if (clicked)
            {
                LogDebug($"[Button]['{label}'] clicked");
                onClick?.Invoke();
            }
            return clicked;
        }

        public static string WrapTextField(string initialValue, string label, RenderTextField renderControl = null, OnTextFieldOnChange onChange = null)
        {
            renderControl ??= StandardTextField();
            var result = renderControl(initialValue, label);
            if (result != initialValue)
            {
                LogDebug($"[TextField]['{label}'] {initialValue} -> {result}");
                onChange?.Invoke(result);
            }
            return result;
        }
    }
}
