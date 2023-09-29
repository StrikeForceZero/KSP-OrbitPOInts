using System;

namespace KSPMock
{
    public class CelestialBody
    {
        public new string name => this.bodyName;
        public double Radius;
        public bool atmosphere;
        public double minOrbitalDistance;
        public double hillSphere;
        public double atmosphereDepth;
        public bool isHomeWorld;
        public string bodyName;

        public MapObject MapObject;

        public double TerrainAltitude(double lati, double longi, bool something)
        {
            throw new NotImplementedException();
        }

        public static bool operator true(CelestialBody body)
        {
            return body != null;
        }

        public static bool operator false(CelestialBody body)
        {
            return body == null;
        }
    }
}
