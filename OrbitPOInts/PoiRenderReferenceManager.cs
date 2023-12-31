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
using UnityEngine;
using UniLinq;
using KSP_CelestialBody = CelestialBody;
#endif
using System;
using System.Collections.Generic;
using OrbitPOInts.Data.POI;
using OrbitPOInts.Extensions;
using OrbitPOInts.Extensions.KSP;
using OrbitPOInts.Extensions.Unity;
using OrbitPOInts.Utils;

namespace OrbitPOInts
{
    using CelestialBody = KSP_CelestialBody;
    using Logger = Utils.Logger;

    public class PoiRenderReferenceManager<TContext> where TContext : MonoBehaviour
    {
        private readonly Dictionary<POI, PoiRenderReference> _poiPoiRenderReferenceDictionary = new(new PoiIdBodyComparer());
        private readonly Dictionary<CelestialBody, HashSet<PoiRenderReference>> _bodyPoiRenderReferenceDictionary = new();

        private readonly TContext _context;

        public PoiRenderReferenceManager(TContext context)
        {
            _context = context;
        }

        public PoiRenderReference GetOrCreatePoiRenderReference(POI poi)
        {
            var poiRenderReferenceOption = TryGetPoiRenderReference(poi);
            if (poiRenderReferenceOption.IsSome)
            {
                LogDebug($"[GetOrCreatePoiRenderReference] {Logger.GetPoiLogId(poi)} reusing");
                return poiRenderReferenceOption.Value;
            }
            LogDebug($"[GetOrCreatePoiRenderReference] {Logger.GetPoiLogId(poi)} creating");
            var poiRenderReference = new PoiRenderReference(poi);
            AddRenderReference(poiRenderReference);
            return poiRenderReference;
        }

        private void AddRenderReference(PoiRenderReference poiRenderReference)
        {
            _poiPoiRenderReferenceDictionary.Add(poiRenderReference.Poi, poiRenderReference);
            var poiRenderReferencesForBody = _bodyPoiRenderReferenceDictionary.GetOrDefault(poiRenderReference.Poi.Body) ?? new HashSet<PoiRenderReference>();
            poiRenderReferencesForBody.Add(poiRenderReference);
            _bodyPoiRenderReferenceDictionary[poiRenderReference.Poi.Body] = poiRenderReferencesForBody;
        }

        private void RemoveRenderReference(PoiRenderReference poiRenderReference, bool destroy = true)
        {
            _poiPoiRenderReferenceDictionary.Remove(poiRenderReference.Poi);
            if (destroy) poiRenderReference.DestroyImmediate();
            var hasBodyEntry =
                _bodyPoiRenderReferenceDictionary.TryGetValue(poiRenderReference.Poi.Body,
                    out var bodyPoiRenderReferences);
            // this shouldn't be false
            if (!hasBodyEntry)
            {
                LogError("PoiRenderReferenceManager's dictionaries are not in sync");
                return;
            };
            bodyPoiRenderReferences.Remove(poiRenderReference);
            // if now empty, remove it from the dictionary
            if (bodyPoiRenderReferences.Count == 0) _bodyPoiRenderReferenceDictionary.Remove(poiRenderReference.Poi.Body);
        }

        public Option<PoiRenderReference> TryGetPoiRenderReference(POI poi)
        {
            return _poiPoiRenderReferenceDictionary.TryGet(poi);
        }

        public IEnumerable<PoiRenderReference> GetPoiRenderReferences(CelestialBody body)
        {
            return _bodyPoiRenderReferenceDictionary.GetOrDefault(body) ?? Enumerable.Empty<PoiRenderReference>();
        }

        public void RemovePoiRenderReference(POI poi, bool destroy = true)
        {
            if (!_poiPoiRenderReferenceDictionary.TryGetValue(poi, out var poiRenderReference))
            {
                LogDebug($"[RemovePoiRenderReference] {Logger.GetPoiLogId(poi)} not found");
                return;
            };
            LogDebug($"[RemovePoiRenderReference] {Logger.GetPoiLogId(poi)} removing");
            RemoveRenderReference(poiRenderReference, destroy);
        }
        
        public void RemovePoiRenderReferences(CelestialBody body, bool destroy = true)
        {
            var poiRenderReferences = _bodyPoiRenderReferenceDictionary.GetOrDefault(body);
            if (poiRenderReferences == null) return;
            foreach (var poiRenderReference in poiRenderReferences)
            {
                _poiPoiRenderReferenceDictionary.Remove(poiRenderReference.Poi);
                if (destroy) poiRenderReference.DestroyImmediate();
            }
            _bodyPoiRenderReferenceDictionary.Remove(body);
        }

        public void Clear(bool destroy = true)
        {
            if (destroy)
            {
                foreach (var poiRenderReference in _poiPoiRenderReferenceDictionary.Values)
                {
                    poiRenderReference.DestroyImmediate();
                }
            }
            _poiPoiRenderReferenceDictionary.Clear();
            _bodyPoiRenderReferenceDictionary.Clear();
        }

        public IEnumerable<PoiRenderReference> AllPoiRenderReferences => _poiPoiRenderReferenceDictionary.Values;
        public IEnumerable<RenderReference<TRender>> GetAllRenderReferences<TRender>() where TRender : MonoBehaviour, IRenderer
        {
            return AllPoiRenderReferences
                .Select(poirr => poirr.GetRenderReference<TRender>())
                .Where(rr => rr != null);
        }

        public IEnumerable<(POI Poi, RenderReference<TRender> RenderReference)> GetAllPoiRenderReferenceTuples<TRender>() where TRender : MonoBehaviour, IRenderer
        {
            return AllPoiRenderReferences
                .Select(poirr => (poirr.Poi, RendererReference: poirr.GetRenderReference<TRender>()))
                .Where(item => item.RendererReference != null);
        }

        public IEnumerable<TRender> GetAllRenderReferenceRenderers<TRender>() where TRender : MonoBehaviour, IRenderer
        {
            return GetAllRenderReferences<TRender>()
                .SelectMany(rr => rr.Renderers);
        }

        public IEnumerable<(POI Poi, TRender Renderer)> GetAllRenderPoiReferenceRenderersTuples<TRender>() where TRender : MonoBehaviour, IRenderer
        {
            return GetAllPoiRenderReferenceTuples<TRender>()
                .SelectMany(item => item.RenderReference.Renderers.Select(renderer => (item.Poi, render: renderer)));
        }

        // TODO: rename?
        public IEnumerable<IRenderer> GetAllRenderReferencesRenderersForPoi(POI poi)
        {
            _poiPoiRenderReferenceDictionary.TryGetValue(poi, out var renderReference);
            return renderReference?.GetRenderers() ?? Enumerable.Empty<IRenderer>();
            // return _poiPoiRenderReferenceDictionary.Values.Where(poirr => poirr.Poi == poi)
            //     .SelectMany(poirr => poirr.GetRenderers());
        }

        public IEnumerable<(PoiRenderReference PoiRenderReference, IRenderer Renderer)> GetAllRenderReferencesRendererTuplesForPoi(POI poi)
        {
            if (!_poiPoiRenderReferenceDictionary.TryGetValue(poi, out var renderReference))
            {
                return Enumerable.Empty<(PoiRenderReference, IRenderer)>();
            }

            var renderers = renderReference?.GetRenderers() ?? Enumerable.Empty<IRenderer>();
            return renderers.Select(renderer => (PoiRenderReference: renderReference, Renderer: renderer));
        }

        private static void LogDebug(string message)
        {
            Logger.LogDebug($"[PoiRenderReferenceManager] {message}");
        }

        private static void Log(string message)
        {
            Logger.Log($"[PoiRenderReferenceManager] {message}");
        }

        private static void LogError(string message)
        {
            Logger.LogError($"[PoiRenderReferenceManager] {message}");
        }
    }
}
