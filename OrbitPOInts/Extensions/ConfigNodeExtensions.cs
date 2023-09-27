using System;
using JetBrains.Annotations;
using OrbitPOInts.Data;
using UnityEngine;

namespace OrbitPOInts.Extensions
{
    public static class ConfigNodeExtensions
    {
        public static string GetString(this ConfigNode configNode, string key, string defaultValue = "")
        {
            return configNode.GetValue(key) ?? defaultValue;
        }

        public static bool GetBool(this ConfigNode configNode, string key, bool defaultValue = false)
        {
            return bool.TryParse(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        public static int GetInt(this ConfigNode configNode, string key, int defaultValue = 0)
        {
            return int.TryParse(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        public static uint GetUInt(this ConfigNode configNode, string key, uint defaultValue = 0)
        {
            return uint.TryParse(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        public static double GetDouble(this ConfigNode configNode, string key, double defaultValue = 0)
        {
            return double.TryParse(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        public static float GetFloat(this ConfigNode configNode, string key, float defaultValue = default)
        {
            return float.TryParse(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        public static Color GetColor(this ConfigNode configNode, string key, Color defaultValue = default)
        {
            return ColorExtensions.TryDeserialize(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        public static PoiType GetPoiType(this ConfigNode configNode, string key, PoiType defaultValue = default)
        {
            return Enum.TryParse(configNode.GetValue(key), out PoiType result) ? result : defaultValue;
        }

        [CanBeNull]
        public static CelestialBody GetCelestialBody(this ConfigNode configNode, string key, CelestialBody defaultValue = default)
        {
            return CelestialBodyExtensions.TryDeserialize(configNode.GetValue(key), out var result) ? result : defaultValue;
        }

        [CanBeNull]
        public static PoiDTO ToPoiDto(this ConfigNode configNode, PoiDTO defaultValue = default)
        {
            return PoiDTO.Load(configNode) ?? defaultValue;
        }

        [CanBeNull]
        public static POI ToPoi(this ConfigNode configNode, POI defaultValue = default)
        {
            return configNode.ToPoiDto()?.ToPoi() ?? defaultValue;
        }

        [CanBeNull]
        public static PoiDTO GetPoiDtoFromNode(this ConfigNode configNode, string key, PoiDTO defaultValue = default)
        {
            var node = configNode.GetNode(key);
            return node?.ToPoiDto() ?? defaultValue;
        }

        [CanBeNull]
        public static POI GetPoiFromNode(this ConfigNode configNode, string key, POI defaultValue = default)
        {
            return configNode.GetPoiDtoFromNode(key)?.ToPoi() ?? defaultValue;
        }
    }
}
