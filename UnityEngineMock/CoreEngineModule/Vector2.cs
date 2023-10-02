using System;

namespace UnityEngineMock
{
    public class Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        // Properties
        public float magnitude => (float)Math.Sqrt(x * x + y * y);
        public Vector2 normalized => new Vector2(x / magnitude, y / magnitude);

        // Operator Overloads
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);

        // Dot Product
        public static float Dot(Vector2 lhs, Vector2 rhs) => lhs.x * rhs.x + lhs.y * rhs.y;

        // ToString for Debugging
        public override string ToString() => $"({x}, {y})";
    }
}
