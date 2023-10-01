using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using OrbitPOInts.Data;
using OrbitPOInts.Data.POI;

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

    public interface INotifyConfiguredPoiPropChanged
    {
        public delegate void NotifyConfiguredPropChangedEventHandler(object sender, object poi, PropertyChangedEventArgs poiPropertyName);
        event NotifyConfiguredPropChangedEventHandler ConfiguredPoiPropChanged;
    }

    public interface INotifyConfiguredPoisCollectionChanged
    {
        event NotifyCollectionChangedEventHandler ConfiguredPoisCollectionChanged;
    }

    public class Settings : INotifyPropertyChanged, INotifyConfiguredPoisCollectionChanged, INotifyConfiguredPoiPropChanged, IDisposable
    {
        public const uint VERSION = 0;

        private static Settings _instance;
        private static readonly object Padlock = new();

        public static event Action<Settings> InstanceCreated;
        public static event Action<Settings> InstanceDestroyed;
        public event PropertyChangedEventHandler PropertyChanged;
        public event INotifyConfiguredPoiPropChanged.NotifyConfiguredPropChangedEventHandler ConfiguredPoiPropChanged;
        public event NotifyCollectionChangedEventHandler ConfiguredPoisCollectionChanged;

        private Settings()
        {
            _configuredPois.CollectionChanged += NotifyCollectionChanged;
            InstanceCreated?.Invoke(this);
        }

        ~Settings()
        {
            Dispose(false);
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            lock (Padlock)
            {
                if (_disposed) return;

                if (_instance == this) _instance = null;

                if (disposing)
                {
                    // Free any other managed objects here.
                    _configuredPois.CollectionChanged -= NotifyCollectionChanged;
                    InstanceDestroyed?.Invoke(this);
                    InstanceDestroyed = null;

                    InstanceCreated = null;
                    PropertyChanged = null;
                    ConfiguredPoiPropChanged = null;
                    ConfiguredPoisCollectionChanged = null;
                }

                // Free any unmanaged objects here, if any.
                _disposed = true;
            }
        }

        public bool IsDisposed => _disposed;

        protected virtual void NotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnPropertyChanged(nameof(ConfiguredPois));
            ConfiguredPoisCollectionChanged?.Invoke(sender, args);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPoiPropChanged(object senderPoi, PropertyChangedEventArgs args)
        {
            ConfiguredPoiPropChanged?.Invoke(this, senderPoi, args);
            if (senderPoi is not POI poi) return;
            if (!IsDefaultPoi(poi)) return;
            RemoveConfiguredPoi(poi);
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
        // only for testing as we don't cleanup or notify
        public static void ResetInstance()
        {
            lock (Padlock)
            {
                _instance = null;
            }
        }
#endif
        // TODO: might be better to just copy the properties from a new object?
        internal static void ResetToDefaults()
        {
            lock (Padlock)
            {
                if (_instance == null) return;
                var oldInstance = _instance;
                _instance.ClearConfiguredPois();
                _instance = null;
                oldInstance.Dispose();
            }
        }


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
                OnPropertyChanged(nameof(GlobalEnable));
            }
        }

        public bool FocusedBodyOnly
        {
            get => _focusedBodyOnly;
            set
            {
                if (_focusedBodyOnly == value) return;
                _focusedBodyOnly = value;
                OnPropertyChanged(nameof(FocusedBodyOnly));
            }
        }

        public bool EnableSpheres
        {
            get => _enableSpheres;
            set
            {
                if (_enableSpheres == value) return;
                _enableSpheres = value;
                OnPropertyChanged(nameof(EnableSpheres));
            }
        }

        public bool AlignSpheres
        {
            get => _alignSpheres;
            set
            {
                if (_alignSpheres == value) return;
                _alignSpheres = value;
                OnPropertyChanged(nameof(AlignSpheres));
            }
        }

        public bool EnableCircles
        {
            get => _enableCircles;
            set
            {
                if (_enableCircles == value) return;
                _enableCircles = value;
                OnPropertyChanged(nameof(EnableCircles));
            }
        }

        public bool ShowPoiMaxTerrainAltitudeOnAtmosphericBodies
        {
            get => _showPoiMaxTerrainAltitudeOnAtmosphericBodies;
            set
            {
                if (_showPoiMaxTerrainAltitudeOnAtmosphericBodies == value) return;
                _showPoiMaxTerrainAltitudeOnAtmosphericBodies = value;
                OnPropertyChanged(nameof(ShowPoiMaxTerrainAltitudeOnAtmosphericBodies));
            }
        }


        public bool LogDebugEnabled { get; set; }

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
        private ObservableCollection<POI> _configuredPois = new();
        public IReadOnlyList<POI> ConfiguredPois => _configuredPois.ToList().AsReadOnly();

        internal void ClearConfiguredPois(IEnumerable<POI> pois = null)
        {
            pois ??= Enumerable.Empty<POI>();
            var oldItems = Enumerable.Empty<POI>();

            if(_configuredPois != null)
            {
                _configuredPois.CollectionChanged -= NotifyCollectionChanged;
                oldItems = _configuredPois.ToList();
            }

            _configuredPois = new ObservableCollection<POI>(pois);
            _configuredPois.CollectionChanged += NotifyCollectionChanged;
            // we are opting to combine Reset + Add into Replace to prevent multiple events for the same action overall
            NotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, pois, oldItems, 0));
        }

        internal void UpdateConfiguredPois(IEnumerable<POI> pois)
        {
            ClearConfiguredPois(pois);
        }

        internal bool HasConfiguredPoi(POI poi)
        {
            return ConfiguredPois.Contains(poi, new PoiSameTargetComparer());
        }

        internal IEnumerable<POI> GetPoisToUpdate(POI updatedPoi, bool updateFirstInstanceOnly = false)
        {
            Func<POI, bool> compareFn = a => PoiSameTargetComparer.StaticEquals(a, updatedPoi);

            // Return all matching POIs if not limited to the first instance.
            if (!updateFirstInstanceOnly) return ConfiguredPois.Where(compareFn);

            var poiToUpdate = ConfiguredPois.FirstOrDefault(compareFn);

            // If no matching POI is found, return an empty enumerable.
            if (poiToUpdate == null) return Enumerable.Empty<POI>();

            // Return an enumerable containing the first matching POI.
            return new[] { poiToUpdate };
        }

        public enum AddConfiguredPoiMethod
        {
            Insert,
            ReplaceFirst,
            ReplaceAll,
        }
        internal void AddConfiguredPoi(POI poi, AddConfiguredPoiMethod addMethod = AddConfiguredPoiMethod.Insert)
        {
            if (addMethod != AddConfiguredPoiMethod.Insert && HasConfiguredPoi(poi))
            {
                var poisToRemove = GetPoisToUpdate(poi, addMethod == AddConfiguredPoiMethod.ReplaceFirst);
                foreach (var poiToRemove in poisToRemove)
                {
                    poiToRemove.PropertyChanged -= OnPoiPropChanged;
                    _configuredPois.Remove(poiToRemove);
                }
            }
            // if its a duplicate we dont want to register twice
            poi.PropertyChanged -= OnPoiPropChanged;
            poi.PropertyChanged += OnPoiPropChanged;
            _configuredPois.Add(poi);
        }

        internal void RemoveConfiguredPoi(POI poi, bool removeFirstOccurrenceOnly = false)
        {
            var poisToRemove = GetPoisToUpdate(poi, removeFirstOccurrenceOnly);
            foreach (var poiToRemove in poisToRemove)
            {
                poi.PropertyChanged -= OnPoiPropChanged;
                _configuredPois.Remove(poiToRemove);
            }
        }

        public IEnumerable<POI> GetConfiguredPoisFor(CelestialBody body)
        {
            return ConfiguredPois.Where(poi => poi.Body == body);
        }

        public static IEnumerable<POI> GetDefaultPoisFor(CelestialBody body)
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

        public static bool IsDefaultPoi(POI poi)
        {
            return GetDefaultPoisFor(poi.Body).Contains(poi, new PoiComparer());
        }
    }
}
