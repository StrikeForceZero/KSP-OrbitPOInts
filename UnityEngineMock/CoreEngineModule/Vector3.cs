using System;

namespace UnityEngineMock
{
    public class Vector3
    {
        public float x;
        public float y;
        public float z;

        public static readonly Vector3 zero = new Vector3(0f, 0f, 0f);
        public static readonly Vector3 one = new Vector3(1f, 1f, 1f);
        public static readonly Vector3 forward = new Vector3(0f, 0f, 1f);
        public static readonly Vector3 back = new Vector3(0f, 0f, -1f);
        public static readonly Vector3 up = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 down = new Vector3(0f, -1f, 0f);
        public static readonly Vector3 left = new Vector3(-1f, 0f, 0f);
        public static readonly Vector3 right = new Vector3(1f, 0f, 0f);

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);

        /*public Vector3 normalized
        {
            get
            {
                float mag = (float)Math.Sqrt(x * x + y * y + z * z);
                return new Vector3(x / mag, y / mag, z / mag);
            }
        }*/

        public Vector3 normalized
        {
            get
            {
                float mag = magnitude;
                if (mag > 1E-05f)
                    return this / mag;
                else
                    return zero;
            }
        }

        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x
            );
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
