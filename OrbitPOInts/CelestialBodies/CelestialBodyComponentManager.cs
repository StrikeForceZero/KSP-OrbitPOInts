using System;
using System.Collections.Generic;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.Unity;
using OrbitPOInts.Utils;
#if TEST
using UnityEngineMock;
using System.Linq;
using KSP_KSPAddon = KSPMock.KSPAddon;
using KSP_HighLogic = KSPMock.HighLogic;
using KSP_CelestialBody = KSPMock.CelestialBody;
using KSP_Vector3d = KSPMock.Vector3d;
using KSP_Vessel = KSPMock.Vessel;
using KSP_FlightGlobals = KSPMock.FlightGlobals;
using KSP_GameEvents = KSPMock.GameEvents;
using KSP_PlanetariumCamera = KSPMock.PlanetariumCamera;
using KSP_MapObject = KSPMock.MapObject;
using KSP_GameScenes = KSPMock.GameScenes;
using KSP_ScaledSpace = KSPMock.ScaledSpace;
#else
using UniLinq;
using UnityEngine;
using KSP_KSPAddon = KSPAddon;
using KSP_HighLogic = HighLogic;
using KSP_CelestialBody = CelestialBody;
using KSP_Vector3d = Vector3d;
using KSP_Vessel = Vessel;
using KSP_FlightGlobals = FlightGlobals;
using KSP_GameEvents = GameEvents;
using KSP_PlanetariumCamera = PlanetariumCamera;
using KSP_MapObject = MapObject;
using KSP_GameScenes = GameScenes;
using KSP_ScaledSpace = ScaledSpace;
#endif

namespace OrbitPOInts.CelestialBodies
{
    using CelestialBody = KSP_CelestialBody;
    using Logger = Utils.Logger;

    public enum ComponentHolderType
    {
        Sphere,
        Circle,
    }

    public class CelestialBodyComponentManager<TContext> where TContext : MonoBehaviour
    {
        private readonly Dictionary<(CelestialBody Body, ComponentHolderType Type), GameObject> _bodyComponentHolders = new();

        private readonly TContext _context;

        public CelestialBodyComponentManager(TContext context)
        {
            _context = context;
        }

        public string GetComponentHolderName(CelestialBody body, ComponentHolderType type)
        {
            return $"component_holder_{body.name}_{type}";
        }

        public GameObject GetOrCreateBodyComponentHolder(CelestialBody body, ComponentHolderType type)
        {
            var componentHolder = TryGetBodyComponentHolder(body, type);
            if (componentHolder.IsSome && componentHolder.Value.IsAlive())
            {
                LogDebug($"[GetOrCreateBodyComponentHolder] Reusing component holder {type} for {body.name}");
                return componentHolder.Value;
            }
            if (componentHolder.IsSome && !componentHolder.Value.IsAlive())
            {
                LogDebug($"[GetOrCreateBodyComponentHolder] Skipping dead component holder for {body.name}");
            }
            LogDebug($"[GetOrCreateBodyComponentHolder] Creating component holder for {body.name}");
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.name = GetComponentHolderName(body, type);
            _bodyComponentHolders.Add((body, type), primitive);
            return primitive;
        }

        public Option<GameObject> TryGetBodyComponentHolder(CelestialBody body, ComponentHolderType componentHolderType)
        {
            return _bodyComponentHolders.TryGet((body, componentHolderType));
        }

        public IEnumerable<Option<GameObject>> TryGetBodyComponentHolders(CelestialBody body)
        {
            return from ComponentHolderType type in Enum.GetValues(typeof(ComponentHolderType)) select TryGetBodyComponentHolder(body, type);
        }

        public IEnumerable<GameObject> TryGetBodyComponentHolders(POI poi)
        {
            if (poi.Body == null) return Enumerable.Empty<GameObject>();
            return TryGetBodyComponentHolders(poi.Body)
                .Where(c => c.IsSome)
                .Select(c => c.Value)
                .Where(c => c.GetComponents<PoiWireSphereRenderer>().Cast<IPoiRenderer>()
                    .Concat(c.GetComponents<PoiCircleRenderer>().Cast<IPoiRenderer>())
                    .Any(r => r.Poi.Equals(poi))
                );
        }

        public IEnumerable<IPoiRenderer> GetRenderersForPoi(POI poi)
        {
            return GetSphereRenderersForPoi(poi).Cast<IPoiRenderer>().Concat(GetCircleRenderersForPoi(poi).Cast<IPoiRenderer>());
        }

        public IEnumerable<PoiWireSphereRenderer> GetSphereRenderersForPoi(POI poi)
        {
            if (poi.Body == null) return Enumerable.Empty<PoiWireSphereRenderer>();
            return TryGetBodyComponentHolders(poi.Body)
                .Where(c => c.IsSome)
                .SelectMany(c => c.Value.GetComponents<PoiWireSphereRenderer>())
                .Where(r => r.Poi.Equals(poi));
        }

        public IEnumerable<PoiCircleRenderer> GetCircleRenderersForPoi(POI poi)
        {
            if (poi.Body == null) return Enumerable.Empty<PoiCircleRenderer>();
            return TryGetBodyComponentHolders(poi.Body)
                .Where(c => c.IsSome)
                .SelectMany(c => c.Value.GetComponents<PoiCircleRenderer>())
                .Where(r => r.Poi.Equals(poi));
        }

        public void RemoveComponentHolders(CelestialBody body, ComponentHolderType componentHolderType)
        {
            var maybeComponentHolder = TryGetBodyComponentHolder(body, componentHolderType);
            if (maybeComponentHolder.IsNone) return;
            var componentHolder = maybeComponentHolder.Value;
            if (!componentHolder.IsAlive()) return;
            switch (componentHolderType)
            {
                case ComponentHolderType.Circle:
                    RemoveCircles(componentHolder, body);
                    break;

                case ComponentHolderType.Sphere:
                    RemoveSpheres(componentHolder, body);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported ComponentHolderType: {componentHolderType}");
            }

            componentHolder.DestroyImmediateIfAlive();
        }

        public void RemoveComponentHolders(CelestialBody body)
        {
            foreach (ComponentHolderType type in Enum.GetValues(typeof(ComponentHolderType)))
            {
                RemoveComponentHolders(body, type);
            }
        }

        public void RemoveSpheres(GameObject componentHolder, CelestialBody body, bool destroy = true)
        {
            foreach (var sphere in componentHolder.GetComponents<PoiWireSphereRenderer>())
            {
                sphere.DestroyImmediateIfAlive();
            }

            _bodyComponentHolders.Remove((body, ComponentHolderType.Sphere));
            if (destroy) componentHolder.DestroyImmediateIfAlive();
        }

        public void RemoveCircles(GameObject componentHolder, CelestialBody body, bool destroy = true)
        {
            foreach (var circle in componentHolder.GetComponents<PoiCircleRenderer>())
            {
                circle.DestroyImmediateIfAlive();
            }
            _bodyComponentHolders.Remove((body, ComponentHolderType.Circle));
            if (destroy) componentHolder.DestroyImmediateIfAlive();
        }

        public void Remove(GameObject componentHolder, CelestialBody body)
        {
            RemoveSpheres(componentHolder, body, false);
            RemoveCircles(componentHolder, body, false);
            componentHolder.DestroyImmediateIfAlive();
        }

        public void Remove(GameObject componentHolder)
        {
            var resolveKvp = _bodyComponentHolders.FirstOrDefault(kvp => kvp.Value == componentHolder);
            if (resolveKvp.Equals(default(KeyValuePair<(CelestialBody Body, ComponentHolderType Type), GameObject>))) throw new ArgumentException("componentHolder not found");
            switch (resolveKvp.Key.Type)
            {
                case ComponentHolderType.Sphere:
                    RemoveSpheres(componentHolder, resolveKvp.Key.Body);
                    break;
                case ComponentHolderType.Circle:
                    RemoveCircles(componentHolder, resolveKvp.Key.Body);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            componentHolder.DestroyImmediateIfAlive();
        }

        public void RemoveAll()
        {
            foreach (var kvp in _bodyComponentHolders)
            {
                Remove(kvp.Value, kvp.Key.Body);
            }
            _bodyComponentHolders.Clear();
        }

        public IEnumerable<GameObject> AllComponentHolders()
        {
            return _bodyComponentHolders.Values;
        }

        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[CelestialBodyComponentManager] {message}");
        }

        private static void Log(string message)
        {
            Logger.Log($"[CelestialBodyComponentManager] {message}");
        }

        private static void LogError(string message)
        {
            Logger.LogError($"[CelestialBodyComponentManager] {message}");
        }
    }
}
