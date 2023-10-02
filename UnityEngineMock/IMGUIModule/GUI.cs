using System;

namespace UnityEngineMock
{
    public class GUI
    {
        public static GUISkin skin;
        public static Color color;

        public static void DragWindow() { throw new NotImplementedException(); }
        public static void DragWindow(Rect dragArea) { throw new NotImplementedException(); }

        public static bool Button(Rect pos, string content) { throw new NotImplementedException(); }

        public delegate void WindowFunction(int id);
    }
}
