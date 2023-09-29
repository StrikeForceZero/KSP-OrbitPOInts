using System;
using UnityEngineMock;

namespace KSPMock
{
    public class CelestialBody : MonoBehaviour, ITargetable, IDiscoverable
    {
        public new string name => this.bodyName;
        public double Radius;
        public bool atmosphere;
        public double minOrbitalDistance;
        public double hillSphere;
        public double atmosphereDepth;
        public double sphereOfInfluence;
        public bool isHomeWorld;
        public string bodyName;
        public string bodyDisplayName;
        // TODO: orbit => this.orbitDriver.orbit
        public Orbit orbit;
        // TODO: OrbitDriver not stubbed
        // public OrbitDriver orbitDriver;

        public MapObject MapObject;

        public double TerrainAltitude(double latitude, double longitude, bool allowNegative)
        {
            throw new NotImplementedException();
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public Vector3 GetObtVelocity()
        {
            throw new NotImplementedException();
        }

        public Vector3 GetSrfVelocity()
        {
            throw new NotImplementedException();
        }

        public Vector3 GetFwdVector()
        {
            throw new NotImplementedException();
        }

        public Vessel GetVessel()
        {
            throw new NotImplementedException();
        }

        public string GetName() => name;

        public string GetDisplayName() => bodyDisplayName;

        public Orbit GetOrbit() => orbit;

        public VesselTargetModes GetTargetingMode()
        {
            throw new NotImplementedException();
        }

        public bool GetActiveTargetable()
        {
            throw new NotImplementedException();
        }

        public string RevealName()
        {
            throw new NotImplementedException();
        }

        public string RevealDisplayName()
        {
            throw new NotImplementedException();
        }

        public double RevealSpeed()
        {
            throw new NotImplementedException();
        }

        public double RevealAltitude()
        {
            throw new NotImplementedException();
        }

        public string RevealSituationString()
        {
            throw new NotImplementedException();
        }

        public string RevealType()
        {
            throw new NotImplementedException();
        }

        public float RevealMass()
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
