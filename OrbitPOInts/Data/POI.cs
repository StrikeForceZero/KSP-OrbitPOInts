using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class POI : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _label;
        private bool _enabled;
        private double _radius;
        private Color _color;
        private CelestialBody _body;
        private bool _addPlanetRadius;
        private float _lineWidth;
        private int _resolution;

        public PoiType Type { get; private set; }

        private string _resolveLabel()
        {
            if (_label is null or "" && Type != PoiType.None)
            {
                return Enum.GetName(typeof(PoiType), Type);
            }

            return _label;
        }

        public string Label
        {
            get => _resolveLabel();
            set
            {
                if (_label == value) return;
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public double Diameter => Radius * 2;

        public double Radius
        {
            get => _radius;
            set
            {
                if (_radius.AreRelativelyEqual(value)) return;
                _radius = value;
                OnPropertyChanged(nameof(Radius));
                // Notifying that Diameter has also changed due to change in Radius.
                OnPropertyChanged(nameof(Diameter));
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value) return;
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        public CelestialBody Body
        {
            get => _body;
            // Body is private set because there’s no current use case to change the Body after creation.
            // Triggering PropertyChanged events for both Radius and Body in the same frame might lead to unintuitive behavior.
            private set
            {
                if (_body == value) return;
                _body = value;
                // Automatically updating Radius here when Type is not Custom might be assigning too much responsibility to this property setter.
                // TODO: Consider refactoring if this leads to issues.
                if (Type != PoiType.Custom)
                {
                    Radius = DefaultRadius(Body);
                }
                OnPropertyChanged(nameof(Body));
            }
        }

        // TODO: should this be part of CustomPOI? problem would be subscribing to property changes
        public bool AddPlanetRadius
        {
            get => _addPlanetRadius;
            set
            {
                if (Type != PoiType.Custom && value) throw new ArgumentException("AddPlanetRadius can only be set to true when Type=PoiType.Custom");
                if (_addPlanetRadius == value) return;
                _addPlanetRadius = value;
                OnPropertyChanged(nameof(AddPlanetRadius));
            }
        }

        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                if (_lineWidth.AreRelativelyEqual(value)) return;
                _lineWidth = value;
                OnPropertyChanged(nameof(LineWidth));
            }
        }

        public int Resolution
        {
            get => _resolution;
            set
            {
                if (_resolution == value) return;
                _resolution = value;
                OnPropertyChanged(nameof(Resolution));
            }
        }

        public POI(PoiType type, CelestialBody body = null)
        {
            Type = type;
            Body = body;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double RadiusForRendering() => AddPlanetRadius ? Radius + Body.Radius : Radius;

        // TODO: need to be careful if the type is Custom it will throw
        public double DefaultRadius(CelestialBody body)
        {
            return GetRadiusForType(body, Type);
        }

        public static POI DefaultFrom(PoiType type)
        {
            Settings.DefaultPoiColors.TryGetValue(type, out var color);
            return new POI(type)
            {
                Color = color,
                Enabled = GetDefaultEnabledForType(type),
                Resolution = GetDefaultResolutionForType(type)
            };
        }

        public static POI DefaultFrom(CelestialBody body, PoiType type)
        {
            var poi = DefaultFrom(type);
            poi.Body = body;
            return poi;
        }

        public POI CloneWith(CelestialBody newBody)
        {
            var dto = PoiDTO.FromPoi(this);
            dto.Color = dto.Color.Clone();
            dto.Body = newBody;
            return dto.ToPoi();
        }

        public static double GetRadiusForType(CelestialBody body, PoiType type)
        {
            return type switch
            {
                PoiType.None => 0,
                PoiType.HillSphere => body.hillSphere,
                PoiType.SphereOfInfluence => body.atmosphereDepth + body.Radius,
                PoiType.Atmosphere => body.atmosphereDepth + body.Radius,
                PoiType.MinimumOrbit => body.minOrbitalDistance + body.Radius,
                PoiType.MaxTerrainAltitude => Lib.GetApproxTerrainMaxHeight(body),
                PoiType.Custom => throw new NotSupportedException("Custom does not have a predefined radius."),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static bool GetDefaultEnabledForType(PoiType type)
        {
            return type switch
            {
                PoiType.None => false,
                PoiType.HillSphere => false,
                PoiType.SphereOfInfluence => true,
                PoiType.Atmosphere => true,
                PoiType.MinimumOrbit => true,
                PoiType.MaxTerrainAltitude => true,
                PoiType.Custom => true,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static int GetDefaultResolutionForType(PoiType type)
        {
            return type switch
            {
                PoiType.None => 0,
                PoiType.HillSphere => 50,
                PoiType.SphereOfInfluence => 50,
                PoiType.Atmosphere => 40,
                PoiType.MinimumOrbit => 50,
                PoiType.MaxTerrainAltitude => 55,
                PoiType.Custom => 50,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }

    class PoiCustomEqualityComparer : IEqualityComparer<POI>
    {
        public static readonly PoiCustomEqualityComparer FilterByType = new PoiCustomEqualityComparer(
            (a, b) => a.Type == b.Type,
            poi => poi.Type.GetHashCode()
        );

        private readonly Func<POI, POI, bool> _compareFn;
        private readonly Func<POI, int> _hashCodeFn;
        public PoiCustomEqualityComparer(Func<POI, POI, bool> compareFn, Func<POI, int> hashCodeFn)
        {
            _compareFn = compareFn;
            _hashCodeFn = hashCodeFn;
        }
        public bool Equals(POI x, POI y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            // return x.Type == y.Type;
            return _compareFn.Invoke(x, y);
        }

        public int GetHashCode(POI obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            // return obj.Type.GetHashCode();
            return _hashCodeFn.Invoke(obj);
        }
    }

    public class PoiComparer : IEqualityComparer<POI>
    {
        public bool Equals(POI x, POI y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(POI obj)
        {
            return StaticGetHashCode(obj);
        }

        public static bool StaticEquals(POI x, POI y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return PoiDTOComparer.StaticEquals(PoiDTO.FromPoi(x), PoiDTO.FromPoi(y));
        }

        public static int StaticGetHashCode(POI obj)
        {
            if (obj == null) return 0;
            return PoiDTOComparer.StaticGetHashCode(PoiDTO.FromPoi(obj));
        }
    }

}
