using KSPMock;

namespace OrbitPOInts.Tests.Mocks
{
    public class MockCelestialBody : CelestialBody
    {
        private readonly ITerrainService _terrainService;

        public MockCelestialBody(ITerrainService terrainService)
        {
            _terrainService = terrainService;
        }

        public override double TerrainAltitude(double latitude, double longitude, bool allowNegative)
        {
            return _terrainService.TerrainAltitude(latitude, longitude, allowNegative);
        }
    }

    public interface ITerrainService
    {
        public double TerrainAltitude(double latitude, double longitude, bool allowNegative);
    }

}
