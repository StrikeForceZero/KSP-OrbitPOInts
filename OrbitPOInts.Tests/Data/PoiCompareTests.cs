using System;
using System.Collections.Generic;
using KSPMock;
using Moq;
using NUnit.Framework;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Tests.Mocks;

namespace OrbitPOInts.Tests.Data
{
#if TEST
    [TestFixture]
    public class PoiCompareTests
    {
        private CelestialBody testBody;
        private CelestialBody testBody2;

        [SetUp]
        public void Setup()
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
                atmosphereDepth = 2,
                Radius = 2,
            };
            FlightGlobals.Bodies = new List<CelestialBody> { testBody, testBody2 };
        }

        [Test]
        public void PoiSameTargetComparer_ShouldMatchCorrectly()
        {
            Func<POI, POI, bool> equals = PoiSameTargetComparer.StaticEquals;

            // same standard types
            Assert.IsTrue(equals(POI.DefaultFrom(PoiType.Atmosphere), POI.DefaultFrom(PoiType.Atmosphere))); // without bodies
            Assert.IsTrue(equals(POI.DefaultFrom(testBody, PoiType.Atmosphere), POI.DefaultFrom(testBody, PoiType.Atmosphere))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.Atmosphere), POI.DefaultFrom(testBody2, PoiType.Atmosphere))); // different bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.Atmosphere), POI.DefaultFrom(PoiType.Atmosphere))); // one without

            // same custom types
            Assert.IsTrue(equals(POI.DefaultFrom(PoiType.Custom), POI.DefaultFrom(PoiType.Custom))); // without bodies
            Assert.IsTrue(equals(POI.DefaultFrom(testBody, PoiType.Custom), POI.DefaultFrom(testBody, PoiType.Custom))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.Custom), POI.DefaultFrom(testBody2, PoiType.Custom))); // different bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.Custom), POI.DefaultFrom(PoiType.Custom))); // one without

            // same none types
            Assert.IsTrue(equals(POI.DefaultFrom(PoiType.None), POI.DefaultFrom(PoiType.None))); // without bodies
            Assert.IsTrue(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(testBody, PoiType.None))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(testBody2, PoiType.None))); // different bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(PoiType.None))); // one without

            // mixed types
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.Custom), POI.DefaultFrom(PoiType.Atmosphere))); // without bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.Custom), POI.DefaultFrom(testBody, PoiType.Atmosphere))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.Custom), POI.DefaultFrom(PoiType.Atmosphere))); // one without
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.Custom), POI.DefaultFrom(testBody, PoiType.Atmosphere))); // one without

            // standard types (with none)
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.None), POI.DefaultFrom(PoiType.Atmosphere))); // without bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(testBody, PoiType.Atmosphere))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(testBody2, PoiType.Atmosphere))); // different bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(PoiType.Atmosphere))); // one without
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.None), POI.DefaultFrom(testBody, PoiType.Atmosphere))); // one without

            // custom types (with none)
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.None), POI.DefaultFrom(PoiType.Custom))); // without bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(testBody, PoiType.Custom))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(testBody2, PoiType.Custom))); // different bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.None), POI.DefaultFrom(PoiType.Custom))); // one without
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.None), POI.DefaultFrom(testBody, PoiType.Custom))); // one without

            // different standard types
            Assert.IsFalse(equals(POI.DefaultFrom(PoiType.SphereOfInfluence), POI.DefaultFrom(PoiType.Atmosphere))); // without bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.SphereOfInfluence), POI.DefaultFrom(testBody, PoiType.Atmosphere))); // with bodies
            Assert.IsFalse(equals(POI.DefaultFrom(testBody, PoiType.SphereOfInfluence), POI.DefaultFrom(PoiType.Atmosphere))); // one without


            // radius check
            var poi1 = POI.DefaultFrom(testBody, PoiType.Custom);
            poi1.Radius = 1;
            var poi2 = POI.DefaultFrom(testBody, PoiType.Custom);
            poi2.Radius = 2;

            Assert.IsFalse(equals(poi1, poi2));

            poi2.Radius = 1;
            Assert.IsTrue(equals(poi1, poi2));
        }
    }
#endif
}
