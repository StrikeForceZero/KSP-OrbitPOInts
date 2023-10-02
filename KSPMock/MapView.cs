using UnityEngineMock;

namespace KSPMock
{
    public class MapView
    {
        public Material orbitLinesMaterial;
        public static MapView fetch => new MapView();
        public static bool MapIsEnabled;
    }
}
