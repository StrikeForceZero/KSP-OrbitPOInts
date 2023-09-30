using System.Collections.Generic;
using OrbitPOInts.Extensions;

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


namespace OrbitPOInts.Data
{
    using CelestialBody = KSP_CelestialBody;
    using ConfigNode = KSP_ConfigNode;

    // TODO: use reflection and attributes to handle all props instead of writing out each one
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
            AddValue(node, () => Body, body => body.Serialize());
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

    public class PoiDTOComparer : IEqualityComparer<PoiDTO>
    {

        public bool Equals(PoiDTO x, PoiDTO y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(PoiDTO obj)
        {
            return StaticGetHashCode(obj);
        }

        public static bool StaticEquals(PoiDTO x, PoiDTO y)
        {
            if (ReferenceEquals(x, y)) return true; // Fast reference equality check.

            if (x == null || y == null) return false; // Quick null check.

            // The conditions are ordered by the likelihood of them being different and
            // their computational cost, from least to most expensive.
            return x.Type == y.Type && // Simple enum comparison, most likely to be different.
                   ReferenceEquals(x.Body, y.Body) && // Reference comparison, likely to be different.
                   x.Radius.AreRelativelyEqual(y.Radius) && // ~7 operations, likely to be different.
                   x.Enabled == y.Enabled &&
                   x.Label == y.Label &&
                   x.Color.Equals(y.Color) && // 4 float comparisons.
                   x.AddPlanetRadius == y.AddPlanetRadius &&
                   x.Resolution == y.Resolution &&
                   x.LineWidth.AreRelativelyEqual(y.LineWidth); // ~7 operations.
        }

        public static int StaticGetHashCode(PoiDTO obj)
        {
            if (obj == null) return 0;

            var hash = 17; // Prime number to start with.
            hash = hash * 31 + (obj.Label?.GetHashCode() ?? 0);
            hash = hash * 31 + obj.Enabled.GetHashCode();
            hash = hash * 31 + obj.Radius.GetHashCode();
            hash = hash * 31 + obj.Color.GetHashCode();
            hash = hash * 31 + obj.Type.GetHashCode();
            hash = hash * 31 + (obj.Body?.GetHashCode() ?? 0);
            hash = hash * 31 + obj.AddPlanetRadius.GetHashCode();
            hash = hash * 31 + obj.LineWidth.GetHashCode();
            hash = hash * 31 + obj.Resolution.GetHashCode();

            return hash;
        }
    }

    // This comparator targets checking equality between Type, Body, and Radius if Type=Custom/None
    public class PoiDTOSameTargetComparer : IEqualityComparer<PoiDTO>
    {

        public bool Equals(PoiDTO x, PoiDTO y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(PoiDTO obj)
        {
            return StaticGetHashCode(obj);
        }

        private static bool ArePoisEquivalent(PoiDTO x, PoiDTO y)
        {
            // Check if types are the same
            if (x.Type != y.Type) return false;
            // but not Custom or None, or if their radii are equal
            return x.Type is not (PoiType.Custom or PoiType.None) || x.Radius.AreRelativelyEqual(y.Radius);
        }

        public static bool StaticEquals(PoiDTO x, PoiDTO y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            // Check if types are the same
            if (x.Type != y.Type) return false;

            // skip Radius check if Custom or None
            if (x.Type is not (PoiType.Custom or PoiType.None)) return true;

            // only thing we care about at this stage is if radius is different for Custom or None
            return x.Radius.AreRelativelyEqual(y.Radius);
        }

        public static int StaticGetHashCode(PoiDTO obj)
        {
            if (obj == null) return 0;

            var hash = 17; // Prime number to start with.
            // hash = hash * 31 + (obj.Label?.GetHashCode() ?? 0);
            // hash = hash * 31 + obj.Enabled.GetHashCode();
            if (obj.Type is not (PoiType.Custom or PoiType.None))
            {
                hash = hash * 31 + obj.Radius.GetHashCode();
            }
            // hash = hash * 31 + obj.Color.GetHashCode();
            hash = hash * 31 + obj.Type.GetHashCode();
            hash = hash * 31 + (obj.Body?.GetHashCode() ?? 0);
            // hash = hash * 31 + obj.AddPlanetRadius.GetHashCode();
            // hash = hash * 31 + obj.LineWidth.GetHashCode();
            // hash = hash * 31 + obj.Resolution.GetHashCode();

            return hash;
        }
    }

}
