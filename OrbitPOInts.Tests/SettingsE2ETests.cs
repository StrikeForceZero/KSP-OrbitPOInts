using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KSPMock;
using Moq;
using NUnit.Framework;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Tests.Mocks;

namespace OrbitPOInts.Tests
{
#if TEST
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class SettingsE2ETests
    {
        private CelestialBody testBody;

        private int defaultPoiPropChangedWhileNotResettingCount = 0;
        private int configuredPoiPropChangedCount = 0;
        private bool _registered;

        private void OnInstanceDestroyed(object senderSettings) => Assert.IsFalse(true, "should not have been called");

        private void OnDefaultPoiOnPropertyChanged(object senderPoi, PropertyChangedEventArgs args)
        {
            var resettablePoi = senderPoi as ResettablePoi;
            Assert.IsNotNull(resettablePoi, "sender is not a ResettablePoi");
            // dont count operations during reset
            if (resettablePoi.IsResetting) return;
            defaultPoiPropChangedWhileNotResettingCount += 1;
        }

        private void OnConfiguredPoiPropChanged(object senderSettings, object senderPoi, PropertyChangedEventArgs args) => configuredPoiPropChangedCount += 1;

        private void Register(bool register = true)
        {
            if (register)
            {
                Assert.IsFalse(_registered, "registered(true) should only be called once per run");
                Settings.InstanceDestroyed += OnInstanceDestroyed;
                foreach (var defaultPoi in Settings.GetAllDefaultPois()) defaultPoi.PropertyChanged += OnDefaultPoiOnPropertyChanged;
                Settings.Instance.ConfiguredPoiPropChanged += OnConfiguredPoiPropChanged;
                _registered = true;
                return;
            }

            Assert.IsTrue(_registered, "registered(false) should only be called once per run");
            Settings.InstanceDestroyed -= OnInstanceDestroyed;
            foreach (var defaultPoi in Settings.GetAllDefaultPois()) defaultPoi.PropertyChanged -= OnDefaultPoiOnPropertyChanged;
            Settings.Instance.ConfiguredPoiPropChanged -= OnConfiguredPoiPropChanged;
            _registered = false;
        }

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
            CelestialBodyCache.ClearCache();
        }

        private void ResetCountersAndFlags()
        {
            defaultPoiPropChangedWhileNotResettingCount = 0;
            configuredPoiPropChangedCount = 0;
        }

        [SetUp]
        public void Setup()
        {
            Settings.ResetInstance();
            ResetCountersAndFlags();
            Register();
        }

        [TearDown]
        public void TearDown()
        {
            Register(false);
        }

        // TODO: This test is bloated
        private void AssertGlobalStateForBody(CelestialBody body)
        {
            ResetCountersAndFlags();

            var tag = $"[Global: {(body ? body.bodyName : "null")}]";

            // sanity check
            Assert.IsEmpty(Settings.Instance.ConfiguredPois, $"{tag}: expected ConfiguredPois to be empty ");

            var targetType = PoiType.Atmosphere;
            Assert.IsTrue(Settings.Instance.GetGlobalEnableFor(body, targetType));

            var globalAtmospherePoi = Settings.Instance.TestGetGlobalPoiFor(body, targetType);
            Assert.IsTrue(Settings.IsDefaultPoi(globalAtmospherePoi), $"{tag}: globalAtmospherePoi is not a default POI");

            var globalAtmosphereResettablePoi = globalAtmospherePoi as ResettablePoi;
            Assert.IsNotNull(globalAtmosphereResettablePoi,  $"{tag}: globalAtmospherePoi is not a ResettablePoi");
            Assert.IsTrue(globalAtmosphereResettablePoi.Sealed,  $"{tag}: globalAtmospherePoi is not sealed");
            Assert.IsTrue(globalAtmospherePoi.Enabled,  $"{tag}: globalAtmospherePoi should be enabled by default");

            globalAtmospherePoi.Enabled = false;

            Assert.GreaterOrEqual(defaultPoiPropChangedWhileNotResettingCount, 1,  $"{tag}: defaultPoiPropChangedWhileNotResettingCount was not called");
            Assert.AreEqual(1, defaultPoiPropChangedWhileNotResettingCount,  $"{tag}: defaultPoiPropChangedWhileNotResettingCount was called more than expected");
            Assert.AreEqual(0, configuredPoiPropChangedCount,  $"{tag}: configuredPoiPropChangedCount was called");
            Assert.IsTrue(globalAtmospherePoi.Enabled,  $"{tag}: globalAtmospherePoi did not reset");

            Assert.IsNotEmpty(Settings.Instance.ConfiguredPois, $"{tag}: globalAtmospherePoi should have been copied into ConfiguredPois");
            Assert.AreEqual(1, Settings.Instance.ConfiguredPois.Count, $"{tag}: ConfiguredPois should only contain 1 poi");
            Assert.IsFalse(Settings.Instance.ConfiguredPois.First().Enabled, $"{tag}: Enabled=false for atmospherePoi did not persist during copy");
            Assert.IsFalse(Settings.Instance.GetGlobalEnableFor(body, targetType),  $"{tag}: Enabled=false for atmospherePoi did not persist or GetGlobalEnableFor is returning wrong result");

            var globalConfiguredAtmospherePoi = Settings.Instance.TestGetGlobalPoiFor(body, targetType);
            Assert.IsFalse(globalConfiguredAtmospherePoi is ResettablePoi,  $"{tag}: globalConfiguredAtmospherePoi should not be a ResettablePoi");
            Assert.IsTrue(PoiSameTargetComparer.StaticEquals(globalAtmospherePoi, globalConfiguredAtmospherePoi),  $"{tag}: globalConfiguredAtmospherePoi should be the same target");
            Assert.IsFalse(PoiComparer.StaticEquals(globalAtmospherePoi, globalConfiguredAtmospherePoi),  $"{tag}: globalConfiguredAtmospherePoi was cloned into ConfiguredPois with same values");
            Assert.IsFalse(globalConfiguredAtmospherePoi.Enabled,  $"{tag}: globalAtmospherePoi was not cloned into ConfiguredPois when we changed Enabled to false");

            Func<POI, bool> whereTargetType = poi => poi.Type == targetType;

            var configuredNull = Settings.Instance.GetConfiguredPoisFor(body).Where(whereTargetType);
            Assert.IsNotNull(configuredNull);
            Assert.AreEqual(1, configuredNull.Count(),  $"{tag}: we only expected 1 configured poi for our testBody");
            Assert.IsFalse(configuredNull.FirstOrDefault().Enabled);

            defaultPoiPropChangedWhileNotResettingCount = 0;


            Assert.IsFalse(Settings.Instance.GetGlobalEnableFor(body, targetType), $"{tag}: impossible state? this should still be false");
            Assert.Contains(globalConfiguredAtmospherePoi, Settings.Instance.ConfiguredPois.ToList(), $"{tag}: globalConfiguredAtmospherePoi should still exist in ConfiguredPois");
            globalConfiguredAtmospherePoi.Enabled = true;
            Assert.IsTrue(globalConfiguredAtmospherePoi.Enabled,  $"{tag}: globalConfiguredAtmospherePoi should not reset");
            Assert.IsEmpty(Settings.Instance.ConfiguredPois, $"{tag}: ConfiguredPois should now be empty; globalConfiguredAtmospherePoi was no longer diverging from default and should have been removed.");
            Assert.IsTrue(Settings.Instance.GetGlobalEnableFor(body, targetType), $"{tag}: globalConfiguredAtmospherePoi was potentially overriden by default");
        }

        [Test]
        public void ConfiguredPois_GetGlobalEnableFor_GlobalStateTest_ReturnsCorrectValue()
        {
            AssertGlobalStateForBody(null);
        }

        [Test]
        public void ConfiguredPois_GetGlobalEnableFor_TestBodyStateTest_ReturnsCorrectValue()
        {
            AssertGlobalStateForBody(testBody);
        }

        [Test]
        public void ConfiguredPois_GetGlobalEnableFor_MultiStateTest_ReturnsCorrectValue()
        {
            AssertGlobalStateForBody(null);
            AssertGlobalStateForBody(null);

            AssertGlobalStateForBody(testBody);
            AssertGlobalStateForBody(testBody);

            AssertGlobalStateForBody(null);
            AssertGlobalStateForBody(testBody);
        }
    }
#endif
}
