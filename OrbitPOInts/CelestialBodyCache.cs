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
        private Dictionary<CelestialBody,double> _bodyToMaxAltitudeDictionary;
        private ReadOnlyDictionary<CelestialBody, double> _bodyToMaxAltitudeDictionaryRO;
        private static HashSet<CelestialBody> _bodyLoggedNoPQS = new ();

        public static CelestialBodyCache Instance => _instance ??= new CelestialBodyCache();
        public IReadOnlyDictionary<string, CelestialBody> NameToBodyDictionary => _nameToBodyDictionary ??= new ReadOnlyDictionary<string, CelestialBody>(FlightGlobals.Bodies.ToDictionary(body => body.name));
        // TODO: might be better to calculate these on demand instead of all on first access
        public IReadOnlyDictionary<CelestialBody,double> BodyToMaxAltitudeDictionary
        {
            get
            {
                UpdateFailed();
                if (_bodyToMaxAltitudeDictionaryRO != null) return _bodyToMaxAltitudeDictionaryRO;
                _bodyToMaxAltitudeDictionary = Initialize();
                _bodyToMaxAltitudeDictionaryRO = new ReadOnlyDictionary<CelestialBody, double>(_bodyToMaxAltitudeDictionary);
                return _bodyToMaxAltitudeDictionaryRO;
            }
        }
        
        private readonly HashSet<CelestialBody> _bodyFailedMaxAltitudeSet = new();

        /// <summary>
        /// Attempts to update the max altitude for bodies that failed to be sampled.
        /// </summary>
        private void UpdateFailed()
        {
            if (_bodyFailedMaxAltitudeSet.Count == 0) return;
            foreach (var body in _bodyFailedMaxAltitudeSet.ToArray())
            {
                UpdateBodyMaxAltitude(body, out var value);
                _bodyToMaxAltitudeDictionary[body] = value;
            } 
        }

        /// <summary>
        /// Attempts to get the max altitude for a body.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool TryGetBodyMaxAltitude(CelestialBody body, out double value)
        {
            value = body.Radius; // fallback
                    
            try
            {
                // Skip bodies without usable PQS (e.g., Sun, some gas giants or not ready)
                if (!body.IsPqsUsable())
                {
                    // Log once per body
                    // Prevents log spam for Sun / Jool
                    if (_bodyLoggedNoPQS.Add(body))
                    {
                        Debug.LogWarning($"[OrbitPOInts] Skipping {body.bodyName} (no usable PQS).");
                    }

                    return false;
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to update the max altitude for a body. Updates the failed set.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="value"></param>
        private void UpdateBodyMaxAltitude(CelestialBody body, out double value)
        {
            if (TryGetBodyMaxAltitude(body, out value))
            {
                _bodyFailedMaxAltitudeSet.Remove(body);
            }
            else
            {
                _bodyFailedMaxAltitudeSet.Add(body);
            }
        }

        /// <summary>
        /// Initializes the body to max altitude dictionary.
        /// </summary>
        /// <returns></returns>
        private Dictionary<CelestialBody,double> Initialize()
        {

            var dict = new Dictionary<CelestialBody,double>();
            var bodies = FlightGlobals.Bodies ?? new List<CelestialBody>();

            foreach (var body in bodies.Where(body => body))
            {
                UpdateBodyMaxAltitude(body, out var value);
                // IMPORTANT: always add the body, even if PQS was not used
                dict[body] = value;
            }
            return dict;
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
