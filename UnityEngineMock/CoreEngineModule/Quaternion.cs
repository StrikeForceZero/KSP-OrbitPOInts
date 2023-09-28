using System;

namespace UnityEngineMock
{
    public class Quaternion
    {
        public float x, y, z, w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion identity => new Quaternion(0, 0, 0, 1f);

        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            float dot = Vector3.Dot(fromDirection, toDirection);

            if (Math.Abs(dot - (-1.0f)) < float.Epsilon)
            {
                // vector a and b point exactly in the opposite direction,
                // so it is a 180 degrees turn around the up-axis
                return new Quaternion(0.0f, 1.0f, 0.0f, Mathf.PI);
            }
            if (Math.Abs(dot - 1.0f) < float.Epsilon)
            {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            }

            float rotAngle = (float) Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(fromDirection, toDirection);
            rotAxis = rotAxis.normalized;

            return AxisAngle(rotAxis, rotAngle);
        }

        public static Quaternion AxisAngle(Vector3 axis, float angle)
        {
            float halfAngle = angle * 0.5f;
            float s = (float)Math.Sin(halfAngle);
            return new Quaternion(axis.x * s, axis.y * s, axis.z * s, (float)Math.Cos(halfAngle));
        }
    }
}
