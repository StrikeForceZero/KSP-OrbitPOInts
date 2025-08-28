using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OrbitPOInts.Extensions.KSP;
using Enumerable = System.Linq.Enumerable;

#if TEST
using System.Linq;
using UnityEngineMock;
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
        private ReadOnlyDictionary<CelestialBody,double> _bodyToMaxAltitudeDictionary;

        public static CelestialBodyCache Instance => _instance ??= new CelestialBodyCache();
        public IReadOnlyDictionary<string, CelestialBody> NameToBodyDictionary => _nameToBodyDictionary ??= new ReadOnlyDictionary<string, CelestialBody>(FlightGlobals.Bodies.ToDictionary(body => body.name));
        // TODO: might be better to calculate these on demand instead of all on first access
        public IReadOnlyDictionary<CelestialBody,double> BodyToMaxAltitudeDictionary
        {
            get
            {
                if (_bodyToMaxAltitudeDictionary != null) return _bodyToMaxAltitudeDictionary;

                var dict = new Dictionary<CelestialBody,double>();
                var bodies = FlightGlobals.Bodies ?? new List<CelestialBody>();

                foreach (var body in bodies.Where(body => body))
                {
                    var value = body.Radius; // fallback
                    
                    try
                    {
                        // Skip bodies without usable PQS (e.g., Sun, some gas giants or not ready)
                        if (!body.IsPqsUsable())
                        {
                            Debug.LogWarning($"[OrbitPOInts] Skipping {body.bodyName} (no usable PQS).");
                            continue;
                        }

                        var max = body.GetApproxTerrainMaxHeight(/*res*/ 32);
                        // store absolute surface radius at max terrain
                        value = body.Radius + Math.Max(0, max);
                        
                        Debug.Log($"[OrbitPOInts] Cached {body.bodyName}: max terrain alt {value:N0} m");
                    }
                    catch (Exception ex)
                    {
                        // Skip bad body instead of failing the whole map
                        Debug.LogError($"[OrbitPOInts] Failed sampling {body?.bodyName ?? "<null>"}: {ex}");
                    }
                    
                    // IMPORTANT: always add the body, even if PQS was not used
                    dict[body] = value;
                }

                _bodyToMaxAltitudeDictionary = new ReadOnlyDictionary<CelestialBody,double>(dict);
                return _bodyToMaxAltitudeDictionary;
            }
        }


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
