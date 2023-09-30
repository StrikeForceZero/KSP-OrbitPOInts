using System;
using System.Collections.Generic;
using System.Linq;
using KSPMock;
using Moq;
using NUnit.Framework;
using OrbitPOInts.Data;
using OrbitPOInts.Tests.Mocks;
using UnityEngineMock;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;
using OrbitPOInts.Extensions.Unity;

namespace OrbitPOInts.Tests
{
#if TEST
    [TestFixture]
    public class ModScenarioTests
    {
        private ModScenario _modScenario;
        private List<POI> _initialPois;

        private JsonSerializerOptions _serializerOptions;

        [SetUp]
        public void Setup()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };
            _serializerOptions.Converters.Add(new ExposedConfigNodeConverter());
            _serializerOptions.Converters.Add(new ConfigNodeConverter());
            _serializerOptions.Converters.Add(new PoiConverter());

            Settings.ResetInstance();
            _modScenario = new ModScenario();

            var mockTerrainService = new Mock<ITerrainService>();
            mockTerrainService.Setup(c => c.TerrainAltitude(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<bool>())).Returns(1);
            var testBody = new MockCelestialBody(mockTerrainService.Object)
            {
                bodyName = "TestBody",
                hillSphere = 1,
                sphereOfInfluence = 1,
                minOrbitalDistance = 1,
                atmosphere = true,
                atmosphereDepth = 1,
                Radius = 1,
            };
            var testBody2 = new MockCelestialBody(mockTerrainService.Object)
            {
                bodyName = "TestBody2",
                hillSphere = 1,
                sphereOfInfluence = 1,
                minOrbitalDistance = 1,
                atmosphere = true,
                atmosphereDepth = 1,
                Radius = 1,
            };
            FlightGlobals.Bodies = new List<CelestialBody> { testBody, testBody2 };

            _initialPois = new List<POI>
            {
                new(PoiType.Atmosphere, testBody) { Enabled = true, Color = Color.green },
                new(PoiType.Custom, testBody) { Enabled = true, Color = Color.black, Radius = 10 },
                new(PoiType.Custom, testBody2) { Enabled = true, Color = Color.red, Radius = 5 },
            };

            Settings.Instance.UpdateConfiguredPois(_initialPois);
        }

        [Test]
        public void ModScenario_LoadConfiguredPois_ShouldLoad()
        {
            var node = new ExposedConfigNode("Root");
            TestContext.WriteLine("Original POIs: ");
            TestContext.WriteLine(JsonSerializer.Serialize(_initialPois, _serializerOptions));
            ModScenario.SaveConfiguredPois(node, _initialPois);
            TestContext.WriteLine("Saved POIs: ");
            TestContext.WriteLine(JsonSerializer.Serialize(node, _serializerOptions));
            var loadedPois = ModScenario.LoadConfiguredPois(node);
            TestContext.WriteLine("Loaded POIs: ");
            TestContext.WriteLine(JsonSerializer.Serialize(loadedPois, _serializerOptions));
            CustomAsserts.CollectionAssert.HaveSameElements(_initialPois, loadedPois, PoiSerializer, new PoiComparer());
        }

        [Test]
        public void ModScenario_SaveConfiguredPois_ShouldSave()
        {
            var node = new ExposedConfigNode("Root");
            TestContext.WriteLine("Original POIs: ");
            TestContext.WriteLine(JsonSerializer.Serialize(_initialPois, _serializerOptions));
            ModScenario.SaveConfiguredPois(node, _initialPois);
            TestContext.WriteLine("Saved Nodes: ");
            TestContext.WriteLine(JsonSerializer.Serialize(node, _serializerOptions));
            var loadedPois = ModScenario.LoadConfiguredPois(node);
            TestContext.WriteLine("Loaded POIs: ");
            TestContext.WriteLine(JsonSerializer.Serialize(loadedPois, _serializerOptions));
            CustomAsserts.CollectionAssert.HaveSameElements(_initialPois, loadedPois, PoiSerializer, new PoiComparer());
        }

        [Test]
        public void ModScenario_OnLoad_ShouldLoad()
        {
            var node = new ExposedConfigNode();
            _modScenario.OnLoad(node);
        }

        [Test]
        public void ModScenario_OnSave_ShouldSave()
        {
            var node = new ExposedConfigNode();
            _modScenario.OnSave(node);
        }

        private static string PoiSerializer(POI poi) => $"POI(body:{poi.Body?.bodyName}, radius:{poi.Radius}, type:{poi.Type})";
    }

    public class ExposedConfigNode : ConfigNode
    {
        public ExposedConfigNode() : base()
        {

        }

        public ExposedConfigNode(string name) : base(name)
        {
        }

        public IReadOnlyDictionary<string, string> GetValueDictionary()
        {
            return this._values;
        }

        public IReadOnlyDictionary<string, IReadOnlyList<ConfigNode>> GetNodeDictionary()
        {
            return this._nodes.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<ConfigNode>)kvp.Value
            );
        }
    }

    public class ExposedConfigNodeConverter : JsonConverter<ExposedConfigNode>
    {
        public override ExposedConfigNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Implement the deserialization logic here
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ExposedConfigNode value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.name);

            // Write values dictionary
            writer.WriteStartObject("values");
            foreach (var kvp in value.GetValueDictionary())
            {
                writer.WriteString(kvp.Key, kvp.Value); // Assumes that the dictionary values are strings.
            }
            writer.WriteEndObject();

            // Write node dictionary
            writer.WriteStartObject("nodes");
            foreach (var kvp in value.GetNodeDictionary())
            {
                JsonSerializer.Serialize(writer, kvp.Value, options); // Assumes that the dictionary values are objects.
                writer.WriteEndObject();
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }

    public class ConfigNodeConverter : JsonConverter<ConfigNode>
    {
        public override ConfigNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Implement the deserialization logic here
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ConfigNode value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.name);

            // Write values dictionary
            writer.WriteStartObject("values");
            foreach (var kvp in value.__GetValueDictionary())
            {
                writer.WriteString(kvp.Key, kvp.Value); // Assumes that the dictionary values are strings.
            }
            writer.WriteEndObject();

            // Write node dictionary
            writer.WriteStartObject("nodes");
            foreach (var kvp in value.__GetNodeDictionary())
            {
                JsonSerializer.Serialize(writer, kvp.Value, options); // Assumes that the dictionary values are objects.
                writer.WriteEndObject();
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }

    public class PoiConverter : JsonConverter<POI>
    {
        public override POI Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Implement the deserialization logic here
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, POI value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("body", value.Body.Serialize());
            writer.WriteString("type", value.Type.ToString());
            writer.WriteString("radius", value.Radius.ToString());
            writer.WriteString("color", value.Color.Serialize());
            writer.WriteEndObject();
        }
    }
#endif
}
