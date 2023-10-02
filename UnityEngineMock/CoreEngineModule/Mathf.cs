using System;

namespace UnityEngineMock
{
    public class Mathf
    {
        public static float Cos(float radians)
        {
            return (float)Math.Cos(radians);
        }

        public static float Sin(float radians)
        {
            return (float)Math.Sin(radians);
        }

        public static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public const float Deg2Rad = (float)(Math.PI / 180.0);

        public const float PI = (float)Math.PI;
    }
}
