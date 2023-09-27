using System;
using System.ComponentModel;
using OrbitPOInts.Extensions;
using UnityEngine;

namespace OrbitPOInts.Data
{

    public class POI : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _label;
        private bool _enabled;
        private double _radius;
        private Color _color;
        private CelestialBody _body;

        public PoiType Type { get; private set; }

        private string _resolveLabel()
        {
            if (_label == "" && Type != PoiType.None)
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

        public double Diameter => _radius * 2;

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

        public POI(PoiType type, CelestialBody body = null)
        {
            Type = type;
            Body = body;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // TODO: need to be careful if the type is Custom it will throw
        public double DefaultRadius(CelestialBody body)
        {
            return GetRadiusForType(body, Type);
        }

        public static POI DefaultFrom(PoiType type)
        {
            Settings.DefaultPoiColors.TryGetValue(type, out var color);
            return new POI(type) { Color = color, Enabled = GetDefaultEnabledForType(type) };
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
    }
}
