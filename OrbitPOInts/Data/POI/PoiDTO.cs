using OrbitPOInts.Data.ConfigNode;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;
using OrbitPOInts.Extensions.Unity;

#if TEST
using UnityEngineMock;
using KSP_ConfigNode = KSPMock.ConfigNode;
using KSP_CelestialBody = KSPMock.CelestialBody;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_ConfigNode = ConfigNode;
using KSP_CelestialBody = CelestialBody;
#endif


namespace OrbitPOInts.Data.POI
{
    using CelestialBody = KSP_CelestialBody;
    using ConfigNode = KSP_ConfigNode;
    public sealed class PoiDTO : ConfigNodeDto<PoiDTO>
    {
        public string Label { get; set; }
        public bool Enabled { get; set; }
        public double Radius { get; set; }
        public Color Color { get; set; }
        public PoiType Type { get; set; }
        public CelestialBody Body { get; set; }
        public bool AddPlanetRadius { get; set; }
        public float LineWidth { get; set; }
        public int Resolution { get; set; }

        private const string ConfigNodeKey = "POI";

        public PoiDTO() : base()
        {

        }

        public PoiDTO(ConfigNode node) : base(node)
        {

        }

        public override ConfigNode Save()
        {
            var node = new ConfigNode(ConfigNodeKey);
            AddValue(node, () => Label);
            AddValue(node, () => Enabled);
            AddValue(node, () => Radius);
            AddValue(node, () => Color, color => color.Serialize());
            AddValue(node, () => Type);
            // don't bother adding the body if its null, it will log an error from ConfigNode.AddValue() and set it to ""
            // while very unlikely that a body will be named "", it might be better to skip it all together
            // TODO: should the responsibility be somewhere else?
            if (Body != null) AddValue(node, () => Body, body => body.Serialize());
            AddValue(node, () => AddPlanetRadius);
            AddValue(node, () => LineWidth);
            AddValue(node, () => Resolution);
            return node;
        }

        protected override void Hydrate(ConfigNode configNode)
        {
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Label, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Enabled, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Radius, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Color, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Type, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Body, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => AddPlanetRadius, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => LineWidth, this);
            ConfigNodeValueExtractor.LoadNamedValueFromNode(configNode, () => Resolution, this);
        }

        public POI ToPoi()
        {
            return ToPoi(this);
        }

        public static POI ToPoi(PoiDTO dto)
        {
            var poi = dto.Type == PoiType.Custom ? new CustomPOI(dto.Body) : new POI(dto.Type, dto.Body);

            poi.Label = dto.Label;
            poi.Enabled = dto.Enabled;
            poi.Radius = dto.Radius;
            poi.Color = dto.Color;
            poi.AddPlanetRadius = dto.AddPlanetRadius;
            poi.LineWidth = dto.LineWidth;
            poi.Resolution = dto.Resolution;

            return poi;
        }

        public static PoiDTO FromPoi(POI poi)
        {
            return new PoiDTO
            {
                Label = poi.Label,
                Enabled = poi.Enabled,
                Radius = poi.Radius,
                Color = poi.Color,
                Type = poi.Type,
                Body = poi.Body,
                AddPlanetRadius = poi.AddPlanetRadius,
                LineWidth = poi.LineWidth,
                Resolution = poi.Resolution,
            };
        }
    }
}
