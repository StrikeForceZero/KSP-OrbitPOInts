using System;
using System.Collections.Generic;
using UnityEngineMock;

namespace KSPMock
{
    public class Vessel : MonoBehaviour, IShipconstruct, ITargetable, IDiscoverable
    {
        public Guid id;
        public string vesselName;
        public Orbit orbit;
        public CelestialBody mainBody;
        public List<Part> Parts { get; }
        public bool loaded;
        public bool Landed;
        public bool Splashed;
        public State state;
        public MapObject mapObject;
        public Transform GetTransform()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetObtVelocity()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetSrfVelocity()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetFwdVector()
        {
            throw new System.NotImplementedException();
        }

        public Vessel GetVessel() => this;

        public string GetName() => this.vesselName;

        // TODO: GetDisplayName() => Localizer.Format(this.vesselName);
        public string GetDisplayName() => throw new NotImplementedException();

        public Orbit GetOrbit() => this.orbit;

        public VesselTargetModes GetTargetingMode()
        {
            throw new System.NotImplementedException();
        }

        public bool GetActiveTargetable()
        {
            throw new System.NotImplementedException();
        }

        public string RevealName()
        {
            throw new System.NotImplementedException();
        }

        public string RevealDisplayName()
        {
            throw new System.NotImplementedException();
        }

        public double RevealSpeed()
        {
            throw new System.NotImplementedException();
        }

        public double RevealAltitude()
        {
            throw new System.NotImplementedException();
        }

        public string RevealSituationString()
        {
            throw new System.NotImplementedException();
        }

        public string RevealType()
        {
            throw new System.NotImplementedException();
        }

        public float RevealMass()
        {
            throw new System.NotImplementedException();
        }

        public enum State
        {
            INACTIVE,
            ACTIVE,
            DEAD,
        }
    }
}
