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
    }
}
