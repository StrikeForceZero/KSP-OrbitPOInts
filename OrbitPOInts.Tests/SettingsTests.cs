using System.Collections.Generic;
using System.Linq;
using KSPMock;
using Moq;
using NUnit.Framework;
using OrbitPOInts.Data;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Tests.Mocks;

namespace OrbitPOInts.Tests
{
#if TEST
    [TestFixture]
    public class SettingsTests
    {
        private CelestialBody testBody;

        [SetUp]
        public void Setup()
        {
            Settings.ResetInstance();
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
        public void ConfiguredPois_InitialState_IsCorrectValue()
        {
            Assert.IsEmpty(Settings.Instance.ConfiguredPois);
        }

        [Test]
        public void ConfiguredPois_ResetInstance_IsWorking()
        {
            var newConfiguredPois = new List<POI> { POI.DefaultFrom(PoiType.Custom) };
            Settings.Instance.UpdateConfiguredPois(newConfiguredPois);
            Assert.IsNotEmpty(Settings.Instance.ConfiguredPois);
            Settings.ResetInstance();
            Assert.IsEmpty(Settings.Instance.ConfiguredPois);
        }

        [Test]
        public void ConfiguredPois_UpdateConfiguredPois_IsSettingCorrectValue()
        {
            var newConfiguredPois = new List<POI> { POI.DefaultFrom(PoiType.Custom) };
            Settings.Instance.UpdateConfiguredPois(newConfiguredPois);
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                newConfiguredPois,
                PoiSerializer,
                new PoiComparer()
            );
        }

        [Test]
        public void ConfiguredPois_UpdateConfiguredPois_Readonly_IsSettingCorrectValue()
        {
            var pois = new List<POI> { POI.DefaultFrom(PoiType.Custom) };
            Settings.Instance.UpdateConfiguredPois(pois.AsReadOnly());
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                pois,
                PoiSerializer,
                new PoiComparer()
            );

            var poi = POI.DefaultFrom(PoiType.Custom);
            Settings.Instance.AddConfiguredPoi(poi);
            pois.Add(poi);
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                pois,
                PoiSerializer,
                new PoiComparer()
            );
        }

        [Test]
        public void ConfiguredPois_UpdateConfiguredPois_FixedSize_IsSettingCorrectValue()
        {
            var pois = new List<POI> { POI.DefaultFrom(PoiType.Custom) };
            Settings.Instance.UpdateConfiguredPois(pois.ToArray());
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                pois,
                PoiSerializer,
                new PoiComparer()
            );

            var poi = POI.DefaultFrom(PoiType.Custom);
            Settings.Instance.AddConfiguredPoi(poi);
            pois.Add(poi);
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                pois,
                PoiSerializer,
                new PoiComparer()
            );
        }

        [Test]
        public void ConfiguredPois_UpdateConfiguredPois_Null_IsSettingCorrectValue()
        {
            Settings.Instance.UpdateConfiguredPois(null);
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                Enumerable.Empty<POI>(),
                PoiSerializer,
                new PoiComparer()
            );

            var poi = POI.DefaultFrom(PoiType.Custom);
            var pois = new List<POI> { poi };
            Settings.Instance.AddConfiguredPoi(poi);
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                pois,
                PoiSerializer,
                new PoiComparer()
            );
        }

        [Test]
        public void ConfiguredPois_AddConfiguredPoi_IsSettingCorrectValue()
        {
            var newPoi = POI.DefaultFrom(PoiType.Custom);
            Settings.Instance.AddConfiguredPoi(newPoi);
            CustomAsserts.CollectionAssert.HaveSameElements(
                Settings.Instance.ConfiguredPois,
                new []{ newPoi },
                PoiSerializer,
                new PoiComparer()
            );
        }

        [Test]
        public void ConfiguredPois_GetCustomPoisFor_Global_ReturnsCorrectValue()
        {
            var newPois = ConfiguredPoiSetup(
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.Custom),
                POI.DefaultFrom(testBody, PoiType.Custom)
            );
            var selectedPois = Settings.Instance.GetCustomPoisFor(null).ToList();
            CustomAsserts.CollectionAssert.HaveSameElements(
                selectedPois,
                new List<POI>() { POI.DefaultFrom(PoiType.Custom) },
                PoiSerializer,
                new PoiComparer()
            );
            // redundant but just in case
            Assert.IsNull(selectedPois.First().Body);
        }

        [Test]
        public void ConfiguredPois_GetCustomPoisFor_Body_ReturnsCorrectValue()
        {
            var newPois = ConfiguredPoiSetup(
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.Custom),
                POI.DefaultFrom(PoiType.SphereOfInfluence),
                POI.DefaultFrom(testBody, PoiType.Custom)
            );
            var selectedPois = Settings.Instance.GetCustomPoisFor(testBody).ToList();
            CustomAsserts.CollectionAssert.HaveSameElements(
                selectedPois,
                new List<POI>() { POI.DefaultFrom(testBody, PoiType.Custom) },
                PoiSerializer,
                new PoiComparer()
            );
            // redundant but just in case
            Assert.AreEqual(testBody, selectedPois.First().Body);
        }

        [Test]
        public void ConfiguredPois_GetStandardPoisFor_Global_ReturnsCorrectValue()
        {
            var newPois = ConfiguredPoiSetup(
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.Custom),
                POI.DefaultFrom(PoiType.SphereOfInfluence),
                POI.DefaultFrom(testBody, PoiType.Custom),
                POI.DefaultFrom(testBody, PoiType.Atmosphere)
            );
            var selectedPois = Settings.Instance.GetStandardPoisFor(null).ToList();
            CustomAsserts.CollectionAssert.HaveSameElements(
                selectedPois,
                GlobalStandardPois,
                PoiSerializer,
                new PoiComparer()
            );
            // redundant but just in case
            Assert.IsNull(selectedPois.First().Body);
        }

        [Test]
        public void ConfiguredPois_GetGlobalEnableFor_Default_ReturnsCorrectValue()
        {
            Assert.IsTrue(Settings.Instance.GetGlobalEnableFor(testBody, PoiType.Atmosphere));
        }

        [Test]
        public void ConfiguredPois_GetGlobalEnableFor_BodyType_ReturnsCorrectValue()
        {
            Assert.IsTrue(Settings.Instance.GetGlobalEnableFor(testBody, PoiType.Atmosphere));
            var pois = ConfiguredPoiSetup(
                POI.DefaultFrom(PoiType.Atmosphere)
            );
            pois.First().Enabled = false;
            Assert.IsFalse(Settings.Instance.GetGlobalEnableFor(testBody, PoiType.Atmosphere));
        }

        [Test]
        public void ConfiguredPois_GetGlobalEnableFor_GlobalAndBodyType_ReturnsCorrectValue()
        {
            ConfiguredPoiSetup(
                new POI(PoiType.Atmosphere) { Enabled = false},
                new POI(PoiType.Atmosphere, testBody) { Enabled = true}
            );
            Assert.IsFalse(Settings.Instance.GetGlobalEnableFor(testBody, PoiType.Atmosphere));
        }

        [Test]
        public void ConfiguredPois_GetStandardPoisFor_Body_ReturnsCorrectValue()
        {
            var newPois = ConfiguredPoiSetup(
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.Custom),
                POI.DefaultFrom(PoiType.SphereOfInfluence),
                POI.DefaultFrom(testBody, PoiType.Custom),
                POI.DefaultFrom(testBody, PoiType.Atmosphere)
            );
            var selectedPois = Settings.Instance.GetStandardPoisFor(testBody).ToList();
            CustomAsserts.CollectionAssert.HaveSameElements(
                selectedPois,
                StandardPoisForTestBody,
                PoiSerializer,
                new PoiComparer()
            );
            // redundant but just in case
            Assert.AreEqual(testBody, selectedPois.First().Body);
        }

        private IList<POI> GlobalStandardPois =>
            new List<POI>()
            {
                POI.DefaultFrom(PoiType.HillSphere),
                POI.DefaultFrom(PoiType.SphereOfInfluence),
                POI.DefaultFrom(PoiType.MinimumOrbit),
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.MaxTerrainAltitude),
            };

        private IList<POI> StandardPoisForTestBody =>
            new List<POI>()
            {
                POI.DefaultFrom(testBody, PoiType.HillSphere),
                POI.DefaultFrom(testBody, PoiType.SphereOfInfluence),
                POI.DefaultFrom(testBody, PoiType.MinimumOrbit),
                POI.DefaultFrom(testBody, PoiType.Atmosphere),
                POI.DefaultFrom(testBody, PoiType.MaxTerrainAltitude),
            };

        private static string PoiSerializer(POI poi) => $"POI(body:{poi.Body?.bodyName}, radius:{poi.Radius}, type:{poi.Type})";

        private static IEnumerable<POI> ConfiguredPoiSetup(params POI[] pois)
        {
            Settings.ResetInstance();
            Settings.Instance.UpdateConfiguredPois(pois);
            return pois.ToList();
        }


    }
#endif
}
