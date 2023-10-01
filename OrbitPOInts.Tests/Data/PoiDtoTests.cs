using System.Collections.Generic;
using KSPMock;
using Moq;
using NUnit.Framework;
using OrbitPOInts.Data;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Tests.Mocks;
using UnityEngineMock;

namespace OrbitPOInts.Tests.Data
{
#if TEST
    [TestFixture]
    public class PoiDtoTests
    {
        private CelestialBody testBody;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var mockTerrainService = new Mock<ITerrainService>();
            mockTerrainService.Setup(c => c.TerrainAltitude(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<bool>())).Returns(1);
            testBody = new MockCelestialBody(mockTerrainService.Object)
            {
                bodyName = "TestBody",
                hillSphere = 1,
                sphereOfInfluence = 1,
                minOrbitalDistance = 1,
                atmosphere = true,
                atmosphereDepth = 1,
                Radius = 1,
            };
            FlightGlobals.Bodies = new List<CelestialBody> { testBody };
        }

        [Test]
        public void PoiDto_ShouldCopy()
        {
            var poiDto = new PoiDTO()
            {
                Body = testBody,
                Type = PoiType.Custom,
                Enabled = true,
                Label = "label",
                Radius = 1,
                AddPlanetRadius = true,
                Color = Color.blue,
                Resolution = 10,
                LineWidth = 2f,
            };

            var poi = poiDto.ToPoi();
            var poi2 = poiDto.ToPoi();

            Assert.That(poi, Is.EqualTo(poi2).Using(new PoiComparer()));

            var poiDto2 = PoiDTO.FromPoi(poi2);
            Assert.That(poiDto2, Is.EqualTo(poiDto).Using(new PoiDTOComparer()));

            var poi3 = poiDto2.ToPoi();
            Assert.That(poi, Is.EqualTo(poi3).Using(new PoiComparer()));

            // ensure references are not maintained
            poiDto.Enabled = false;
            Assert.IsTrue(poi2.Enabled);
        }
    }
#endif
}
