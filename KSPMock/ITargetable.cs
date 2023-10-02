using UnityEngineMock;

namespace KSPMock
{
    public interface ITargetable
    {
        Transform GetTransform();

        Vector3 GetObtVelocity();

        Vector3 GetSrfVelocity();

        Vector3 GetFwdVector();

        Vessel GetVessel();

        string GetName();

        string GetDisplayName();

        Orbit GetOrbit();

        // TODO: OrbitDriver not stubbed
        // OrbitDriver GetOrbitDriver();

        VesselTargetModes GetTargetingMode();

        bool GetActiveTargetable();
    }
}
