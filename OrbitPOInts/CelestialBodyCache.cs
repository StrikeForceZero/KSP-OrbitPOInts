using System.Collections.Generic;
using System.Collections.ObjectModel;
using OrbitPOInts.Extensions.KSP;

#if TEST
using System.Linq;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
using JB_Annotations = UnityEngineMock.JetBrains.Annotations;
#else
using UnityEngine;
using UniLinq;
using KSP_CelestialBody = CelestialBody;
using KSP_FlightGlobals = FlightGlobals;
using JB_Annotations = JetBrains.Annotations;
#endif

namespace OrbitPOInts
{
    using CelestialBody = KSP_CelestialBody;
    using FlightGlobals = KSP_FlightGlobals;
    using CanBeNull = JB_Annotations.CanBeNullAttribute;

    public class CelestialBodyCache
    {
        private static CelestialBodyCache _instance;
        private IReadOnlyDictionary<string, CelestialBody> _nameToBodyDictionary;
        private IReadOnlyDictionary<CelestialBody, double> _bodyToMaxAltitudeDictionary;

        public static CelestialBodyCache Instance => _instance ??= new CelestialBodyCache();
        public IReadOnlyDictionary<string, CelestialBody> NameToBodyDictionary => _nameToBodyDictionary ??= new ReadOnlyDictionary<string, CelestialBody>(FlightGlobals.Bodies.ToDictionary(body => body.name));
        // TODO: might be better to calculate these on demand instead of all on first access
        public IReadOnlyDictionary<CelestialBody, double> BodyToMaxAltitudeDictionary => _bodyToMaxAltitudeDictionary ??= new ReadOnlyDictionary<CelestialBody, double>(FlightGlobals.Bodies.ToDictionary(body => body, body => body.GetApproxTerrainMaxHeight() + body.Radius));

        private CelestialBodyCache()
        {
        }

        [CanBeNull]
        public CelestialBody ResolveByName([CanBeNull] string name)
        {
            if (name == null)
            {
                return null;
            }
            NameToBodyDictionary.TryGetValue(name, out var result);
            return result;
        }

        public static void ClearCache()
        {
            _instance = null;
        }
    }
}
