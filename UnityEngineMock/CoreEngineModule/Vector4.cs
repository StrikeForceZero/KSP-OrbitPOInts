using System;

namespace UnityEngineMock
{
    public class Vector4 : IEquatable<Vector4>
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        // Equality Comparison
        public override bool Equals(object obj)
        {
            return obj is Vector4 other && Equals(other);
        }

        public bool Equals(Vector4 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                hash = hash * 23 + z.GetHashCode();
                hash = hash * 23 + w.GetHashCode();
                return hash;
            }
        }

        // Basic Operations
        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Vector4 operator *(Vector4 a, float scalar)
        {
            return new Vector4(a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
        }

        public static Vector4 operator /(Vector4 a, float scalar)
        {
            return new Vector4(a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
        }

        // Dot Product
        public static float Dot(Vector4 a, Vector4 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        // Magnitude and Normalization
        public float Magnitude() => (float)Math.Sqrt(x * x + y * y + z * z + w * w);

        public Vector4 Normalized()
        {
            float mag = Magnitude();
            if (mag == 0) return new Vector4(0, 0, 0, 0);
            return this / mag;
        }
    }

}
