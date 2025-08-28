using UnityEngineMock;

namespace KSPMock
{
    public class PQS : MonoBehaviour
    {
        // needs to be set for tests expecting PQS to be active
        public bool isActiveAndEnabled = true;

        public double GetSurfaceHeight(double radial)
        {
            return 0;
        }
    }
}
