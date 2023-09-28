using System;

namespace UnityEngineMock
{
    public class Color
    {
        public float r = 0f;
        public float g = 0f;
        public float b = 0f;
        public float a = 1f;

        public Color() {}

        public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public string ToString()
        {
            throw new NotImplementedException("TODO");
        }

        public static Color white => new Color(1f, 1f, 1f); // White
        public static Color black => new Color(0f, 0f, 0f); // Black
        public static Color magenta => new Color(1f, 0f, 1f); // Magenta
        public static Color red => new Color(1f, 0f, 0f); // Red
        public static Color green => new Color(0f, 1f, 0f); // Green
        public static Color blue => new Color(0f, 0f, 1f); // Blue
        public static Color yellow => new Color(1f, 0.92f, 0.016f); // Yellow
        public static Color cyan => new Color(0f, 1f, 1f); // Cyan
        public static Color gray => new Color(0.5f, 0.5f, 0.5f); // Gray
        public static Color clear => new Color(0f, 0f, 0f, 0f); // clear
    }
}
