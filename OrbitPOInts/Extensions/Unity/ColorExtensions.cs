#if TEST
using UnityEngineMock;

#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.Extensions.Unity
{
    public static class ColorExtensions
    {
        private const uint R = 0;
        private const uint G = 1;
        private const uint B = 2;
        private const uint A = 3;

        private static float ColorComponentFromInt(int value)
        {
            return Mathf.Clamp01(value / 255f);
        }

        private static int ColorComponentToInt(float value)
        {
            return Mathf.Clamp((int)(value * 255), 0, 255);
        }

        public static bool TryDeserialize(string input, out Color result)
        {
            result = Color.clear;
            if (string.IsNullOrEmpty(input)) return false;

            var colorComponents = input.Split(',');
            if (colorComponents.Length < 3) return false;

            if (int.TryParse(colorComponents[R], out var r) &&
                int.TryParse(colorComponents[G], out var g) &&
                int.TryParse(colorComponents[B], out var b))
            {
                var a = 255; // default alpha value
                if (colorComponents.Length == 4 && !int.TryParse(colorComponents[A], out a))
                {
                    return false;
                }

                result = new Color(
                    ColorComponentFromInt(r),
                    ColorComponentFromInt(g),
                    ColorComponentFromInt(b),
                    ColorComponentFromInt(a)
                );
                return true;
            }

            return false;
        }

        public static string Serialize(this Color color)
        {
            var r = ColorComponentToInt(color.r);
            var g = ColorComponentToInt(color.g);
            var b = ColorComponentToInt(color.b);
            var a = ColorComponentToInt(color.a);

            // Omitting the alpha value if it's 255 (fully opaque)
            return a == 255
                ? $"{r},{g},{b}"
                : $"{r},{g},{b},{a}";
        }

        public static Color Clone(this Color source)
        {
            return new Color(source.r, source.g, source.b, source.a);
        }
    }
}
