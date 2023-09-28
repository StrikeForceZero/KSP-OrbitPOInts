using UnityEngineMock;

namespace KSPMock
{
    public class Vector3d
    {
        public double x;
        public double y;
        public double z;

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3d(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.z = 0.0;
        }

        public Vector3d xzy => new Vector3d(this.x, this.z, this.y);

        public static implicit operator Vector3(Vector3d v) => new Vector3((float) v.x, (float) v.y, (float) v.z);

        public static implicit operator Vector3d(Vector3 v) => new Vector3d((double) v.x, (double) v.y, (double) v.z);
    }
}
