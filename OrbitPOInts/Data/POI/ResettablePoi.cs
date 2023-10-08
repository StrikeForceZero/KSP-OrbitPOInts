using System;
using System.ComponentModel;

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

namespace OrbitPOInts.Data.POI
{
    using CelestialBody = KSP_CelestialBody;

    public class ResettablePoi : POI
    {
        public bool Dirty { get; private set; }
        public bool Sealed { get; private set; }
        public POI SealedState { get; private set; }

        public bool IsResetting { get; private set; }

        private bool _isInitialized;

        public ResettablePoi(POI poiSource, bool seal = false) : base(poiSource.Type, poiSource.Body)
        {
            CopyFromPoi(poiSource);
            Initialize(seal);
        }

        public ResettablePoi(PoiType type, CelestialBody body = null, bool seal = false) : base(type, body)
        {
            Initialize(seal);
        }

        private void Initialize(bool seal)
        {
            if (_isInitialized) throw new InvalidOperationException("Initialize should only be called once.");

            PropertyChanged += OnBasePropertyChanged;
            if (seal) Seal();
            _isInitialized = true;
        }

        public static ResettablePoi From(POI sourcePoi, bool seal = false)
        {
            return new ResettablePoi(sourcePoi, seal);
        }

        private void OnBasePropertyChanged(object senderPoi, PropertyChangedEventArgs args)
        {
            if (!Sealed || IsResetting) return;
            Dirty = true;
        }

        public void Seal()
        {
            if (Sealed) return;
            Sealed = true;
            SealedState = Clone();
        }

        public void Reset()
        {
            if (!Dirty) return;

            // if this is a problem we can add a lock
            if (IsResetting) throw new InvalidOperationException("Tried calling Reset() during reset!");

            // so we dont trigger a property change event and set Dirty=true
            IsResetting = true;

            CopyFromPoi(SealedState);

            IsResetting = false;

            Dirty = false;
        }

        public void Save()
        {
            if (!Dirty) return;

            // if this is a problem we can add a lock
            if (IsResetting) throw new InvalidOperationException("Tried calling Save() during reset!");
            SealedState = this;
            Dirty = false;
        }

        private void CopyFromPoi(POI poi)
        {
            Label = poi.Label;
            Enabled = poi.Enabled;
            Radius = poi.Radius;
            Color = poi.Color;
            Type = poi.Type;
            Body = poi.Body;
            AddPlanetRadius = poi.AddPlanetRadius;
            LineWidth = poi.LineWidth;
            Resolution = poi.Resolution;
        }
    }
}
