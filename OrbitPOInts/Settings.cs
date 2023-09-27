using System;
using System.Collections.Generic;
using OrbitPOInts.Data;
using OrbitPOInts.Extensions;
using UniLinq;
using UnityEngine;

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

        private static bool _enablePOI_HillSphere;
        private static bool _enablePOI_SOI = true;
        private static bool _enablePOI_Atmo = true;
        private static bool _enablePOI_MinOrbit = true;
        private static bool _enablePOI_MaxAlt = true;
        private static bool _showPoiMaxAltOnAtmoBodies;

        private static bool _customPoisFromCenter;
        private static double _customPoi1;
        private static bool _customPoi1Enabled;
        private static double _customPoi2;
        private static bool _customPoi2Enabled;
        private static double _customPoi3;
        private static bool _customPoi3Enabled;

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

        public static bool EnablePOI_HillSphere
        {
            get => _enablePOI_HillSphere;
            set
            {
                if (_enablePOI_HillSphere == value) return;
                _enablePOI_HillSphere = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnablePOI_SOI
        {
            get => _enablePOI_SOI;
            set
            {
                if (_enablePOI_SOI == value) return;
                _enablePOI_SOI = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnablePOI_Atmo
        {
            get => _enablePOI_Atmo;
            set
            {
                if (_enablePOI_Atmo == value) return;
                _enablePOI_Atmo = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnablePOI_MinOrbit
        {
            get => _enablePOI_MinOrbit;
            set
            {
                if (_enablePOI_MinOrbit == value) return;
                _enablePOI_MinOrbit = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool EnablePOI_MaxAlt
        {
            get => _enablePOI_MaxAlt;
            set
            {
                if (_enablePOI_MaxAlt == value) return;
                _enablePOI_MaxAlt = value;
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

        public static bool CustomPOiFromCenter
        {
            get => _customPoisFromCenter;
            set
            {
                if (_customPoisFromCenter == value) return;
                _customPoisFromCenter = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static double CustomPOI1
        {
            get => _customPoi1;
            set
            {
                if (_customPoi1.AreRelativelyEqual(value)) return;
                _customPoi1 = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool CustomPOI1Enabled
        {
            get => _customPoi1Enabled;
            set
            {
                if (_customPoi1Enabled == value) return;
                _customPoi1Enabled = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static double CustomPOI2
        {
            get => _customPoi2;
            set
            {
                if (_customPoi2.AreRelativelyEqual(value)) return;
                _customPoi2 = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool CustomPOI2Enabled
        {
            get => _customPoi2Enabled;
            set
            {
                if (_customPoi2Enabled == value) return;
                _customPoi2Enabled = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static double CustomPOI3
        {
            get => _customPoi3;
            set
            {
                if (_customPoi3.AreRelativelyEqual(value)) return;
                _customPoi3 = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public static bool CustomPOI3Enabled
        {
            get => _customPoi3Enabled;
            set
            {
                if (_customPoi3Enabled == value) return;
                _customPoi3Enabled = value;
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
