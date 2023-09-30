using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OrbitPOInts.Data;

#if TEST
using UnityEngineMock;
using System.Linq;
using KSP_CelestialBody = KSPMock.CelestialBody;
#else
using UniLinq;
using UnityEngine;
using KSP_CelestialBody = CelestialBody;
#endif

namespace OrbitPOInts
{

    using CelestialBody = KSP_CelestialBody;

    public class Settings
    {
        public const uint VERSION = 0;

        private static Settings _instance;
        private static readonly object Padlock = new();

        private Settings()
        {
        }

        public static Settings Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Padlock)
                {
                    _instance ??= new Settings();
                }
                return _instance;
            }
        }

#if TEST
        public static void ResetInstance()
        {
            lock (Padlock)
            {
                _instance = null;
            }
        }
#endif


        private bool _globalEnable = true;

        private bool _focusedBodyOnly = true;

        private bool _enableSpheres = true;
        private bool _alignSpheres;
        private bool _enableCircles = true;

        private bool _showPoiMaxTerrainAltitudeOnAtmosphericBodies;

        public bool GlobalEnable
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

        public bool FocusedBodyOnly
        {
            get => _focusedBodyOnly;
            set
            {
                if (_focusedBodyOnly == value) return;
                _focusedBodyOnly = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }

        public bool EnableSpheres
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

        public bool AlignSpheres
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

        public bool EnableCircles
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

        public bool ShowPoiMaxTerrainAltitudeOnAtmosphericBodies
        {
            get => _showPoiMaxTerrainAltitudeOnAtmosphericBodies;
            set
            {
                if (_showPoiMaxTerrainAltitudeOnAtmosphericBodies == value) return;
                _showPoiMaxTerrainAltitudeOnAtmosphericBodies = value;
                OrbitPoiVisualizer.Instance.CurrentTargetRefresh();
            }
        }


        public bool LogDebugEnabled { get; set; }

        [Obsolete]
        public readonly IDictionary<PoiType, Color> FakePoiColors = new Dictionary<PoiType, Color>
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
        private IList<POI> _configuredPois = new List<POI>();
        public IReadOnlyList<POI> ConfiguredPois => _configuredPois.ToList().AsReadOnly();

        internal void UpdateConfiguredPois(IList<POI> pois)
        {
            _configuredPois = pois;
        }

        internal void AddPoi(POI poi)
        {
            _configuredPois.Add(poi);
        }

        public IEnumerable<POI> GetConfiguredPoisFor(CelestialBody body)
        {
            return ConfiguredPois.Where(poi => poi.Body == body);
        }

        private IEnumerable<POI> GetDefaultPoisFor(CelestialBody body)
        {
            return DefaultGlobalPoiDictionary.Values.Select(poi => poi.CloneWith(body));
        }

        public IEnumerable<POI> GetCustomPoisFor(CelestialBody body)
        {
            return GetConfiguredPoisFor(body) // configured body
                .Where(poi => poi.Type == PoiType.Custom); // priority: configured only
        }

        public IEnumerable<POI> GetStandardPoisFor(CelestialBody body)
        {
            return GetConfiguredPoisFor(body) // configured body
                .Union(GetDefaultPoisFor(body), PoiCustomEqualityComparer.FilterByType)  // default
                .Where(poi => poi.Type != PoiType.Custom); // custom is not standard poi
            // priority: configured > default
        }

        public POI GetStandardPoiFor(CelestialBody body, PoiType poiType)
        {
            return GetConfiguredPoisFor(body) // configured body
                .Concat(GetDefaultPoisFor(body)) // defaults
                .FirstOrDefault(poi => poi.Type == poiType); // priority: configured > default
        }
        
        public bool GetGlobalEnableFor(CelestialBody body, PoiType poiType)
        {
            return GetConfiguredPoisFor(null) // configured globals
                .Concat(GetConfiguredPoisFor(body)) // configured body
                .Concat(GetDefaultPoisFor(body)) // defaults
                .FirstOrDefault(poi => poi.Type == poiType) // priority: global > configured > default
                .Enabled;
        }
    }
}
