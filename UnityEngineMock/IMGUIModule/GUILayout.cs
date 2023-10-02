using System;

namespace UnityEngineMock
{
    public class GUILayout
    {
        public static void BeginVertical() { throw new NotImplementedException(); }
        public static void EndVertical() { throw new NotImplementedException(); }
        public static void BeginHorizontal() { throw new NotImplementedException(); }
        public static void EndHorizontal() { throw new NotImplementedException(); }
        public static void Label(string text) { throw new NotImplementedException(); }
        public static float HorizontalSlider(float initial, float min, float max) { throw new NotImplementedException(); }
        public static GUILayoutOption Width(uint width) { throw new NotImplementedException(); }
        public static GUILayoutOption Height(uint height) { throw new NotImplementedException(); }
        public static void Box(object content, GUIStyle style, params GUILayoutOption[] options) { throw new NotImplementedException(); }
        public static void Space(uint space) { throw new NotImplementedException(); }
        public static bool Button(string text) { throw new NotImplementedException(); }
        public static bool Button(string text, params GUILayoutOption[] options) { throw new NotImplementedException(); }
        public static Rect Window(uint id, Rect windowRect, GUI.WindowFunction drawUiAction, string title) { throw new NotImplementedException(); }
        public static Rect Window(uint id, Rect windowRect, GUI.WindowFunction drawUiAction, string title, GUIStyle windowStyle) { throw new NotImplementedException(); }
        public static void FlexibleSpace() { throw new NotImplementedException(); }
        public static bool Toggle(bool initial, object content) { throw new NotImplementedException(); }
        public static bool Toggle(bool initial, object content, params GUILayoutOption[] options) { throw new NotImplementedException(); }
        public static bool Toggle(bool initial, object content, GUIStyle style, params GUILayoutOption[] options) { throw new NotImplementedException(); }
        public static GUILayoutOption ExpandWidth(bool expand) { throw new NotImplementedException(); }
        public static string TextField(string text, params GUILayoutOption[] options) { throw new NotImplementedException(); }

        public static int SelectionGrid(int selected, string[] texts, int xCount, params GUILayoutOption[] options) { throw new NotImplementedException(); }

    }
}
