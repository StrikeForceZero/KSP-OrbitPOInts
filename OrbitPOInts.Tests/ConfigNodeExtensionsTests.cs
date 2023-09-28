using NUnit.Framework;
using OrbitPOInts.Data;
using OrbitPOInts.Extensions;
using OrbitPOInts.Wrappers;

using KSPMock;
using UnityEngineMock;
using System.Linq;

namespace OrbitPOInts.Tests
{
    [TestFixture]
    public class ConfigNodeExtensionsTests
    {
        private ConfigNode _configNode;

        private CelestialBody _celestialBodyReference;


        [SetUp]
        public void SetUp()
        {
            foreach (var body in FakeFlightGlobalsWrapper.Instance.Bodies)
            {
                _celestialBodyReference = body;
            }
            _configNode = new ConfigNode();
            _configNode.SetValue("boolKey", "true");
            _configNode.SetValue("intKey", "123");
            _configNode.SetValue("uintKey", "456");
            _configNode.SetValue("doubleKey", "789.123");
            _configNode.SetValue("floatKey", "321.123f");
            _configNode.SetValue("colorKey", Color.white.Serialize());
            _configNode.SetValue("poiTypeKey", PoiType.Custom.ToString());
            _configNode.SetValue("celestialBodyKey", _celestialBodyReference.Serialize());
        }


        [Test]
        public void GetString_ReturnsCorrectValue()
        {
            string result = _configNode.GetString("someKey", "defaultValue");
            Assert.AreEqual("defaultValue", result);
        }

        [Test]
        public void GetBool_ReturnsCorrectValue()
        {
            bool result = _configNode.GetBool("boolKey", false);
            Assert.IsTrue(result);
        }

        [Test]
        public void GetInt_ReturnsCorrectValue()
        {
            int result = _configNode.GetInt("intKey", 0);
            Assert.AreEqual(123, result);
        }

        [Test]
        public void GetUInt_ReturnsCorrectValue()
        {
            uint result = _configNode.GetUInt("uintKey", 0);
            Assert.AreEqual(456u, result);
        }

        [Test]
        public void GetDouble_ReturnsCorrectValue()
        {
            double result = _configNode.GetDouble("doubleKey", 0);
            Assert.AreEqual(789.123, result);
        }

        [Test]
        public void GetFloat_ReturnsCorrectValue()
        {
            float result = _configNode.GetFloat("floatKey", 0);
            Assert.AreEqual(321.123f, result);
        }

        [Test]
        public void GetColor_ReturnsCorrectValue()
        {
            Color expected = Color.white;
            Color result = _configNode.GetColor("colorKey", Color.black);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetPoiType_ReturnsCorrectValue()
        {
            PoiType result = _configNode.GetPoiType("poiTypeKey", default);
            Assert.AreEqual(PoiType.Custom, result);
        }

        [Test]
        public void GetCelestialBody_ReturnsCorrectValue()
        {
            var result = _configNode.GetCelestialBody("celestialBodyKey", default);
            Assert.AreEqual(_celestialBodyReference, result);
        }
    }
}
