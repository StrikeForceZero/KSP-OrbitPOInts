using System;
using System.Collections.Generic;
using KSPMock;
using Moq;
using NUnit.Framework;
using OrbitPOInts.Extensions.KSP;
using OrbitPOInts.Tests.Mocks;

namespace OrbitPOInts.Tests
{
#if TEST
    [TestFixture]
    public class CelestialBodyCacheTests
    {
        private CelestialBody testBody;

        [OneTimeSetUp]
        public void OneTimeSetup()
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

        [SetUp]
        public void Setup()
        {
            CelestialBodyCache.ClearCache();
        }

        [Test]
        public void CelestialBodyCache_ShouldCache()
        {
            var resolvedBody = CelestialBodyCache.Instance.ResolveByName("TestBody");
            Assert.AreEqual(testBody, resolvedBody);

            var maxAltitude = testBody.GetApproxTerrainMaxHeight() + testBody.Radius;
            Assert.AreEqual(maxAltitude, CelestialBodyCache.Instance.BodyToMaxAltitudeDictionary[testBody]);
        }

        [Test]
        public void CelestialBodyCache_ShouldNotThrowWhenResolvingNull()
        {
            Func<CelestialBody> resolve = () => CelestialBodyCache.Instance.ResolveByName(null);
            TestDelegate resolveThrowTest = () => resolve();
            Assert.DoesNotThrow(resolveThrowTest);
            Assert.IsNull(resolve());
        }

    }
#endif
}
