using OrbitPOInts.Extensions;
using UnityEngineMock;

namespace OrbitPOInts.Data.POI
{
    public sealed class PoiDTO : ConfigNodeDto<PoiDTO>
    {
        public string Label { get; set; }
        public bool Enabled { get; set; }
        public double Radius { get; set; }
        public Color Color { get; set; }
        public PoiType Type { get; set; }
        public KSPMock.CelestialBody Body { get; set; }
        public bool AddPlanetRadius { get; set; }
        public float LineWidth { get; set; }
        public int Resolution { get; set; }

        private const string ConfigNodeKey = "POI";

        public PoiDTO() : base()
        {

        }

        public PoiDTO(KSPMock.ConfigNode node) : base(node)
        {

        }

        public override KSPMock.ConfigNode Save()
        {
            var node = new KSPMock.ConfigNode(ConfigNodeKey);
            AddValue(node, () => Label);
            AddValue(node, () => Enabled);
            AddValue(node, () => Radius);
            AddValue(node, () => Color, color => color.Serialize());
            AddValue(node, () => Type);
            AddValue(node, () => Body, body => body.Serialize());
            AddValue(node, () => AddPlanetRadius);
            AddValue(node, () => LineWidth);
            AddValue(node, () => Resolution);
            return node;
        }

        protected override void Hydrate(KSPMock.ConfigNode configNode)
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