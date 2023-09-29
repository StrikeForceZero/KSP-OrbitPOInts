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

        public override int GetHashCode() => ((Vector4) this).GetHashCode();

        public override bool Equals(object other) => other is Color other1 && this.Equals(other1);

        public bool Equals(Color other) => this.r.Equals(other.r) && this.g.Equals(other.g) && this.b.Equals(other.b) && this.a.Equals(other.a);

        public static Color operator +(Color a, Color b) => new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);

        public static Color operator -(Color a, Color b) => new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);

        public static Color operator *(Color a, Color b) => new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);

        public static Color operator *(Color a, float b) => new Color(a.r * b, a.g * b, a.b * b, a.a * b);

        public static Color operator *(float b, Color a) => new Color(a.r * b, a.g * b, a.b * b, a.a * b);

        public static Color operator /(Color a, float b) => new Color(a.r / b, a.g / b, a.b / b, a.a / b);

        public static bool operator ==(Color lhs, Color rhs) {
            if (ReferenceEquals(lhs, rhs)) return true; // Both are null or same reference.
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) return false; // One of them is null.

            return ((Vector4) lhs).Equals((Vector4) rhs);
        }

        public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);

        public static implicit operator Vector4(Color c) => new Vector4(c.r, c.g, c.b, c.a);

        public static implicit operator Color(Vector4 v) => new Color(v.x, v.y, v.z, v.w);
    }
}
