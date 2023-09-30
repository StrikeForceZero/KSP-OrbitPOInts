using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OrbitPOInts.Data.ConfigNode.Extensions;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;
using UnityEngineMock.JetBrains.Annotations;
#if TEST
using UnityEngineMock;
using KSP_ConfigNode = KSPMock.ConfigNode;
using KSP_CelestialBody = KSPMock.CelestialBody;

#else
using UniLinq;
using UnityEngine;
using KSP_ConfigNode = ConfigNode;
using KSP_CelestialBody = CelestialBody;
using JetBrains.Annotations;
#endif

namespace OrbitPOInts.Data.ConfigNode
{
    using ConfigNode = KSP_ConfigNode;
    using CelestialBody = KSP_CelestialBody;

    public static class ConfigNodeValueExtractor
    {
        private delegate T ExtractorDelegate<T>(ConfigNode node, string key, T defaultValue);

        private static KeyValuePair<Type, Delegate> CreateExtractor<T>(ExtractorDelegate<T> extractor) => new(typeof(T), extractor);

        private static string ExtractString(ConfigNode node, string key, string defaultValue) => node.GetString(key, defaultValue);

        private static bool ExtractBool(ConfigNode node, string key, bool defaultValue) => node.GetBool(key, defaultValue);

        private static int ExtractInt(ConfigNode node, string key, int defaultValue) => node.GetInt(key, defaultValue);

        private static uint ExtractUInt(ConfigNode node, string key, uint defaultValue) => node.GetUInt(key, defaultValue);

        private static double ExtractDouble(ConfigNode node, string key, double defaultValue) => node.GetDouble(key, defaultValue);

        private static float ExtractFloat(ConfigNode node, string key, float defaultValue) => node.GetFloat(key, defaultValue);

        private static Color ExtractColor(ConfigNode node, string key, Color defaultValue) => ColorExtensions.TryDeserialize(node.GetValue(key), out var result) ? result : defaultValue;

        private static PoiType ExtractPoiType(ConfigNode node, string key, PoiType defaultValue) => node.GetPoiType(key, defaultValue);

        [CanBeNull]
        private static CelestialBody ExtractCelestialBody(ConfigNode node, string key, CelestialBody defaultValue) => CelestialBodyExtensions.TryDeserialize(node.GetValue(key), out var result) ? result : defaultValue;


        private static Dictionary<Type, Delegate> InitializeExtractorDictionary(Dictionary<Type, Delegate> dictionary)
        {
            dictionary.AddKeyValuePair(CreateExtractor<string>(ExtractString));
            dictionary.AddKeyValuePair(CreateExtractor<bool>(ExtractBool));
            dictionary.AddKeyValuePair(CreateExtractor<int>(ExtractInt));
            dictionary.AddKeyValuePair(CreateExtractor<uint>(ExtractUInt));
            dictionary.AddKeyValuePair(CreateExtractor<double>(ExtractDouble));
            dictionary.AddKeyValuePair(CreateExtractor<float>(ExtractFloat));
            dictionary.AddKeyValuePair(CreateExtractor<Color>(ExtractColor));
            dictionary.AddKeyValuePair(CreateExtractor<PoiType>(ExtractPoiType));
            dictionary.AddKeyValuePair(CreateExtractor<CelestialBody>(ExtractCelestialBody));
            return dictionary;
        }

        private static readonly Dictionary<Type, Delegate> Extractors = InitializeExtractorDictionary(new Dictionary<Type, Delegate>());

        public static T ExtractValue<T>(ConfigNode node, string key, T defaultValue = default)
        {
            if (Extractors.TryGetValue(typeof(T), out var extractor))
            {
                return (T)extractor.DynamicInvoke(node, key, defaultValue);
            }

            throw new NotSupportedException($"The type {typeof(T)} is not supported for extraction from ConfigNode.");
        }

        public static T ExtractValueNamed<T>(ConfigNode node, Expression<Func<T>> propertyExpression, T defaultValue = default)
        {
            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException($"The provided expression should point to a valid property of type {typeof(T).Name}.", nameof(propertyExpression));
            }

            var propertyName = member.Member.Name;
            return ExtractValue(node, propertyName, defaultValue);
        }

        public static void LoadNamedValueFromNode<T, TValue>(ConfigNode node, Expression<Func<TValue>> propertyExpression, T target, TValue defaultValue = default)
        {
            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException("The provided expression did not point to a valid property.", nameof(propertyExpression));
            }

            if (member.Member is not PropertyInfo propertyInfo) throw new InvalidOperationException($"The provided member '{member.Member.Name}' is not a property of type '{typeof(T).Name}'.");

            var propertyName = member.Member.Name;

            object value = ExtractValue(node, propertyName, defaultValue);
            propertyInfo.SetValue(target, value);
        }

    }
}
