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
    public class PoiTests
    {
        private CelestialBody testBody;
        private CelestialBody testBody2;

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
            testBody2 = new MockCelestialBody(mockTerrainService.Object)
            {
                bodyName = "TestBody2",
                hillSphere = 2,
                sphereOfInfluence = 2,
                minOrbitalDistance = 2,
                atmosphere = false,
                atmosphereDepth = 0,
                Radius = 2,
            };
            FlightGlobals.Bodies = new List<CelestialBody> { testBody, testBody2 };
        }

        [Test]
        public void POI_RadiusForRendering_ShouldConditionallyAddPlanetRadius()
        {
            var radius = 5;
            var poi = new POI(PoiType.Custom, testBody)
            {
                Radius = radius,
            };
            Assert.AreEqual(poi.RadiusForRendering(), radius);
            poi.AddPlanetRadius = true;
            Assert.AreEqual(poi.RadiusForRendering(), radius + poi.Body.Radius);
        }

        [Test]
        public void POI_CloneWith_ShouldCloneNull()
        {
            var poi = new POI(PoiType.Atmosphere)
            {
                Enabled = true,
                Label = "label",
                Radius = 0,
                AddPlanetRadius = false,
                Color = Color.blue,
                Resolution = 10,
                LineWidth = 2f,
            };
            Assert.IsNull(poi.Body);

            var poi2 = poi.CloneWith(testBody);
            Assert.AreEqual(POI.GetRadiusForType(poi2.Body, poi2.Type), poi2.Radius);
            Assert.That(poi, Is.Not.EqualTo(poi2).Using(new PoiComparer()));

            var poi3 = poi.CloneWith(testBody2);
            Assert.AreEqual(POI.GetRadiusForType(poi3.Body, poi3.Type), poi3.Radius);
            Assert.That(poi, Is.Not.EqualTo(poi3).Using(new PoiComparer()));

            Assert.That(poi2, Is.Not.EqualTo(poi3).Using(new PoiComparer()));
        }

        [Test]
        public void POI_CloneWith_ShouldClone()
        {
            var poi = new POI(PoiType.Custom, testBody)
            {
                Enabled = true,
                Label = "label",
                Radius = 1,
                AddPlanetRadius = true,
                Color = Color.blue,
                Resolution = 10,
                LineWidth = 2f,
            };

            var poi2 = poi.CloneWith(testBody2);
            Assert.That(poi, Is.Not.EqualTo(poi2).Using(new PoiComparer()));

            var poi3 = poi2.CloneWith(testBody);
            Assert.That(poi, Is.EqualTo(poi3).Using(new PoiComparer()));
        }

        [Test]
        public void POI_Clone_ShouldClone()
        {
            var poi = new POI(PoiType.Custom, testBody)
            {
                Enabled = true,
                Label = "label",
                Radius = 1,
                AddPlanetRadius = true,
                Color = Color.blue,
                Resolution = 10,
                LineWidth = 2f,
            };

            var poi2 = poi.Clone();
            Assert.That(poi, Is.EqualTo(poi2).Using(new PoiComparer()));
        }
    }
#endif
}
