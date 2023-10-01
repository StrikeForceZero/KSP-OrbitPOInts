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

        public ResettablePoi(POI poiSource, bool seal = false) : base(poiSource.Type, poiSource.Body)
        {
            CopyFromPoi(poiSource);
            if (!seal) return;
            Seal();
        }

        public ResettablePoi(PoiType type, CelestialBody body = null) : base(type, body)
        {
            PropertyChanged += OnBasePropertyChanged;
        }

        public static ResettablePoi From(POI sourcePoi, bool seal = false)
        {
            return new ResettablePoi(sourcePoi, seal);
        }

        private void OnBasePropertyChanged(object senderPoi, PropertyChangedEventArgs args)
        {
            if (!Sealed) return;
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

            CopyFromPoi(SealedState);

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
