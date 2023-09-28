using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OrbitPOInts.Data;
using UniLinq;
using UnityEngine;

namespace OrbitPOInts
{
    // TODO: this is lazy, come up with a better way later
    public static class Settings
    {
        public const uint VERSION = 0;

        private static bool _globalEnable = true;

        private static bool _focusedBodyOnly = true;

        private static bool _enableSpheres = true;
        private static bool _alignSpheres;
        private static bool _enableCircles = true;

        private static bool _showPoiMaxTerrainAltitudeOnAtmosphericBodies;

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

        public static bool FocusedBodyOnly
        {
            get => _focusedBodyOnly;
            set
            {
                if (_focusedBodyOnly == value) return;
                _focusedBodyOnly = value;
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

        public static bool ShowPoiMaxTerrainAltitudeOnAtmosphericBodies
        {
            get => _showPoiMaxTerrainAltitudeOnAtmosphericBodies;
            set
            {
                if (_showPoiMaxTerrainAltitudeOnAtmosphericBodies == value) return;
                _showPoiMaxTerrainAltitudeOnAtmosphericBodies = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }


        public static bool LogDebugEnabled { get; set; }

        [Obsolete]
        public static readonly IDictionary<PoiType, Color> FakePoiColors = new Dictionary<PoiType, Color>
        {
            { PoiType.HillSphere, Color.white },
            { PoiType.SphereOfInfluence, Color.magenta },
            { PoiType.Atmosphere, Color.cyan },
            { PoiType.MinimumOrbit, Color.green },
            { PoiType.MaxTerrainAltitude, Color.red },
            { PoiType.Custom, Color.white },
        };

        public static readonly IReadOnlyDictionary<PoiType, Color> DefaultPoiColors = new ReadOnlyDictionary<PoiType, Color>(
            new Dictionary<PoiType, Color> {
                { PoiType.HillSphere, Color.white },
                { PoiType.SphereOfInfluence, Color.magenta },
                { PoiType.Atmosphere, Color.cyan },
                { PoiType.MinimumOrbit, Color.green },
                { PoiType.MaxTerrainAltitude, Color.red },
                { PoiType.Custom, Color.white },
            }
        );

        private static IDictionary<PoiType, POI> CreatePoiTypeDictionary(params POI[] pois)
        {
            return pois.ToDictionary(poi => poi.Type);
        }

        public static readonly IReadOnlyDictionary<PoiType, POI> DefaultGlobalPoiDictionary = new ReadOnlyDictionary<PoiType, POI>(
            CreatePoiTypeDictionary(
                POI.DefaultFrom(PoiType.HillSphere),
                POI.DefaultFrom(PoiType.SphereOfInfluence),
                POI.DefaultFrom(PoiType.Atmosphere),
                POI.DefaultFrom(PoiType.MinimumOrbit),
                POI.DefaultFrom(PoiType.MaxTerrainAltitude)
            )
        );

        // user configured
        public static readonly IList<POI> ConfiguredPois = new List<POI>();

        public static IEnumerable<POI> GetConfiguredPoisFor(CelestialBody body)
        {
            return ConfiguredPois.Where(poi => poi.Body == body);
        }

        private static IEnumerable<POI> GetDefaultPoisFor(CelestialBody body)
        {
            return DefaultGlobalPoiDictionary.Values.Select(poi => poi.CloneWith(body));
        }

        public static IReadOnlyList<POI> GetCustomPoisFor(CelestialBody body)
        {
            return GetConfiguredPoisFor(body) // configured body
                .Where(poi => poi.Type == PoiType.Custom)
                .ToList()
                .AsReadOnly(); // priority: configured > default
        }

        public static POI GetStandardPoiFor(CelestialBody body, PoiType poiType)
        {
            return GetConfiguredPoisFor(body) // configured body
                .Concat(GetDefaultPoisFor(body)) // defaults
                .FirstOrDefault(poi => poi.Type == poiType); // priority: configured > default
        }
        
        public static bool GetGlobalEnableFor(CelestialBody body, PoiType poiType)
        {
            return GetConfiguredPoisFor(null) // configured globals
                .Concat(GetConfiguredPoisFor(body)) // configured body
                .Concat(GetDefaultPoisFor(body)) // defaults
                .FirstOrDefault(poi => poi.Type == poiType) // priority: global > configured > default
                .Enabled;
        }
    }
}
