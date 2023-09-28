using System;
using System.Collections.Generic;
using System.Configuration;
using OrbitPOInts.Data;
using OrbitPOInts.Extensions;
using UniLinq;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;

namespace OrbitPOInts
{
    using PoiTypePoiListDictionary = Dictionary<PoiType, List<POI>>; // can't use aliases of the same level
    using PoiConfig = Dictionary<string, Dictionary<PoiType, List<POI>>>;
    using PoiConfigEntry = KeyValuePair<string, Dictionary<PoiType, List<POI>>>;

    // TODO: this is lazy, come up with a better way later
    public static class Settings
    {
        public const uint VERSION = 0;

        private static bool _globalEnable = true;

        private static bool _activeBodyOnly = true;

        private static bool _enableSpheres = true;
        private static bool _alignSpheres;
        private static bool _enableCircles = true;

        private static bool _showPoiMaxAltOnAtmoBodies;

        public static bool GlobalEnable
        {
            get => _globalEnable;
            set
            {
                if (_globalEnable == value) return;
                _globalEnable = value;
                OrbitPoiVisualizer.Instance.enabled = value;
                if (OrbitPoiVisualizer.Instance.enabled) OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool ActiveBodyOnly
        {
            get => _activeBodyOnly;
            set
            {
                if (_activeBodyOnly == value) return;
                _activeBodyOnly = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnableSpheres
        {
            get => _enableSpheres;
            set
            {
                if (_enableSpheres == value) return;
                _enableSpheres = value;
                OrbitPoiVisualizer.Instance.DrawSpheres = value;
                OrbitPoiVisualizer.Instance.PurgeSpheres();
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool AlignSpheres
        {
            get => _alignSpheres;
            set
            {
                if (_alignSpheres == value) return;
                _alignSpheres = value;
                OrbitPoiVisualizer.Instance.AlignSpheres = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnableCircles
        {
            get => _enableCircles;
            set
            {
                if (_enableCircles == value) return;
                _enableCircles = value;
                OrbitPoiVisualizer.Instance.DrawCircles = value;
                OrbitPoiVisualizer.Instance.PurgeCircles();
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool ShowPOI_MaxAlt_OnAtmoBodies
        {
            get => _showPoiMaxAltOnAtmoBodies;
            set
            {
                if (_showPoiMaxAltOnAtmoBodies == value) return;
                _showPoiMaxAltOnAtmoBodies = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }


        public static bool LogDebugEnabled { get; set; }

        [Obsolete]
        public static readonly Dictionary<PoiType, Color> FakePoiColors = new()
        {
            { PoiType.HillSphere, Color.white },
            { PoiType.SphereOfInfluence, Color.magenta },
            { PoiType.Atmosphere, Color.cyan },
            { PoiType.MinimumOrbit, Color.green },
            { PoiType.MaxTerrainAltitude, Color.red },
            { PoiType.Custom, Color.white },
        };

        public static readonly Dictionary<PoiType, Color> DefaultPoiColors = new()
        {
            { PoiType.HillSphere, Color.white },
            { PoiType.SphereOfInfluence, Color.magenta },
            { PoiType.Atmosphere, Color.cyan },
            { PoiType.MinimumOrbit, Color.green },
            { PoiType.MaxTerrainAltitude, Color.red },
            { PoiType.Custom, Color.white },
        };

        public static PoiTypePoiListDictionary CreatePoiDictionary(params POI[] pois)
        {
            var dict = new Dictionary<PoiType, List<POI>>();
            foreach(var poi in pois)
            {
                if(!dict.ContainsKey(poi.Type))
                {
                    dict[poi.Type] = new List<POI>();
                }
                dict[poi.Type].Add(poi);
            }
            return dict;
        }

        public static PoiTypePoiListDictionary DefaultPoiListDictionary =>
            CreatePoiDictionary(
                POI.DefaultFrom(PoiType.HillSphere),
                POI.DefaultFrom(PoiType.SphereOfInfluence),
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.MinimumOrbit),
                POI.DefaultFrom(PoiType.MaxTerrainAltitude)
            );
        public const string GlobalSettingsKey = "__GLOBAL__";
        public static PoiConfig PoiConfig = LoadDefault(
            new PoiConfigEntry(
                GlobalSettingsKey,
                DefaultPoiListDictionary
            )
        );

        public static IReadOnlyDictionary<PoiType, List<POI>> GetGlobalPoiConfig()
        {
            return PoiConfig[GlobalSettingsKey];
        }

        public static bool GetGlobalPoiConfigEnabledByPoiType(PoiType type)
        {
            var hasType = GetGlobalPoiConfig().TryGetValue(type, out var pois);
            if (!hasType) throw new ArgumentException($"{type} is not in global config!");
            if (pois == null || pois.Count == 0) throw new InvalidOperationException($"{type} does not have any entries! (list is null or empty)");
            return Enumerable.First(pois).Enabled;
        }

        private static PoiConfig LoadDefault(PoiConfigEntry defaultGlobalEntry)
        {
            PoiConfig poiConfig = new();
            poiConfig.AddKeyValuePair(defaultGlobalEntry);

            foreach (var body in FlightGlobals.Bodies)
            {
                // Clone the default PoiListDictionary for the current body
                var bodyPoiDictionary = DefaultPoiListDictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(poi => poi.CloneWith(body)).ToList()
                );

                // Serialize the body and add the PoiListDictionary to the config
                poiConfig.Add(body.Serialize()!, bodyPoiDictionary);
            }

            return poiConfig;
        }

        // readonly because we arent saving it back when creating a new one
        public static IReadOnlyDictionary<PoiType, List<POI>> GetPOIsForBody(CelestialBody body)
        {
            // TODO: body.serialize?
            var hasBody = PoiConfig.TryGetValue(body.name, out var poiTypePoiListDictionary);
            // TODO: throw?
            if (!hasBody) poiTypePoiListDictionary = new PoiTypePoiListDictionary();
            return poiTypePoiListDictionary;
        }

        // readonly because we arent saving it back when creating a new one
        public static IReadOnlyList<POI> GetPoisForPoiType(CelestialBody body, PoiType type, bool addMissingDefaults = false)
        {
            if (type == PoiType.None) throw new ArgumentException($"Can't use PoiType.None for type parameter");
            var hasType = GetPOIsForBody(body).TryGetValue(type, out var pois);
            if (!hasType) pois = new List<POI>();
            if (!addMissingDefaults) return pois;
            if (pois.Count == 0)
            {
                pois.Add(POI.DefaultFrom(body, type));
            }
            return pois;
        }

        public static POI GetStandardPoi(CelestialBody body, PoiType type)
        {
            var pois = GetPoisForPoiType(body, type, true);
            return pois.First();
        }
    }

    class PoiTypePoiListDictionaryDto : ConfigNodeDto<PoiTypePoiListDictionaryDto>
    {
        public PoiTypePoiListDictionary Dictionary { get; set; }

        public override ConfigNode Save()
        {
            var dictionaryNode = new ConfigNode("Dictionary");
            foreach (var kvp in Dictionary)
            {
                var listNode = new ConfigNode("List");
                foreach (var poi in kvp.Value)
                {
                    listNode.AddNode(PoiDTO.FromPoi(poi).Save());
                }
                dictionaryNode.AddNode(kvp.Key.ToString(), listNode);
            }
            return dictionaryNode;
        }

        private void IteratePoiList(ConfigNode[] pois, Action<ConfigNode> onEachPoi = null, Action<ConfigNode, ConfigNode.Value> onEachValue = null)
        {
            if (onEachPoi == null && onEachValue == null) return;
            foreach (var poi in pois)
            {
                onEachPoi?.Invoke(poi);
                if (onEachValue == null) continue;
                foreach (ConfigNode.Value value in poi.values)
                {
                    onEachValue?.Invoke(poi, value);
                }
            }
        }

        protected override void Hydrate(ConfigNode dictionaryNode)
        {
            Dictionary = new PoiTypePoiListDictionary();

            foreach (var listNode in dictionaryNode.GetNodes())
            {
                var pois = listNode.GetNodes();
                if (Enum.TryParse(listNode.name, true, out PoiType type))
                {
                    Logger.LogError($"Failed to parse POI type {listNode.name}");
                    IteratePoiList(pois, poi =>
                        {
                            Logger.Log($"failed to load: {poi.name} ");
                        },
                        (poi, value) =>
                        {
                            Logger.Log($"{value.name}.{value.value}");
                        }
                    );
                    Logger.Log($"");
                    continue;
                }
                var list = new List<POI>();
                IteratePoiList(pois, poi =>
                {
                    list.Add(PoiDTO.Load(poi).ToPoi());
                });
                Dictionary.Add(type, list);
            }
        }
    }
}
