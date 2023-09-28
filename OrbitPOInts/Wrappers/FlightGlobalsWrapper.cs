using System.Collections.Generic;

#if TEST
using UnityEngineMock;
using System.Linq;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
#else
using UniLinq;
using UnityEngine;
using KSP_CelestialBody = CelestialBody;
using KSP_FlightGlobals = FlightGlobals;
#endif

namespace OrbitPOInts.Wrappers
{
    using CelestialBody = KSP_CelestialBody;
    using FlightGlobals = KSP_FlightGlobals;

    public interface IFlightGlobals
    {
        List<CelestialBody> Bodies { get; }
    }

    public class FlightGlobalsWrapper : IFlightGlobals
    {
        protected static readonly object Padlock = new object();
        protected static FlightGlobalsWrapper _instance;
        public static FlightGlobalsWrapper Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (Padlock)
                {
                    _instance ??= new FlightGlobalsWrapper();
                }

                return _instance;
            }
        }

        public FlightGlobals Unwrap => FlightGlobals.fetch;
        public virtual List<CelestialBody> Bodies => FlightGlobals.Bodies;
    }

    public class FakeFlightGlobalsWrapper : FlightGlobalsWrapper, IFlightGlobals
    {
        private readonly GameObject _gameObject = new GameObject("TestObject");

        public static FlightGlobalsWrapper Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Padlock)
                {
                    _instance ??= new FakeFlightGlobalsWrapper();
                }
                return _instance;
            }
        }

        private List<CelestialBody> _bodies;
        public override List<CelestialBody> Bodies => _bodies.ToList();

        private CelestialBody CreateTestBody(string bodyName)
        {
            var celestialBody = _gameObject.AddComponent<CelestialBody>();
            celestialBody.bodyName = bodyName;
            return celestialBody;
        }

        public FakeFlightGlobalsWrapper()
        {
            _bodies = new List<CelestialBody>
            {
                CreateTestBody("TestBody"),
            };
        }

        ~FakeFlightGlobalsWrapper()
        {
            Object.DestroyImmediate(_gameObject);
        }
    }
}
