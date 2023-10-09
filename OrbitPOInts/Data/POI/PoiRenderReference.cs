
using System;
using System.Collections.Generic;
using OrbitPOInts.Extensions.Unity;
#if TEST
using UnityEngineMock;
using GameObject = UnityEngineMock.GameObject;
using JB_Annotations = UnityEngineMock.JetBrains.Annotations;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using GameObject = UnityEngine.GameObject;
using JB_Annotations = JetBrains.Annotations;
#endif

namespace OrbitPOInts.Data.POI
{
    using CanBeNullAttribute = JB_Annotations.CanBeNullAttribute;

    public class PoiRenderReference
    {
        // TODO: this POI reference is not guaranteed to be the original
        // thanks to all the cloning hacks we have going around...
        public POI Poi { get; private set;  }

        [CanBeNull]
        public RenderReference<WireSphereRenderer> Sphere { get; private set; }

        [CanBeNull]
        public RenderReference<CircleRenderer> Circle { get; private set; }

        public PoiRenderReference(POI poi)
        {
            Poi = poi;
        }

        // TODO: this a bit weird exposing this
        // and we should probably include a function reference that we call when this is called to fix the renderers
        public void UpdatePoi(POI poi)
        {
            Poi = poi;
        }

        private static void DynamicReferenceDestroy<TRenderer>([CanBeNull] RenderReference<TRenderer> oldRenderReference, RenderReference<TRenderer> newRenderReference) where TRenderer : MonoBehaviour, IRenderer
        {
            if (oldRenderReference == null) return;
            if (oldRenderReference.Root == newRenderReference.Root)
            {
                oldRenderReference.RenderersDestroyImmediate();
            }
            else
            {
                oldRenderReference.DestroyImmediate();
            }
        }

        [CanBeNull]
        public RenderReference<TRender> GetRenderReference<TRender>() where TRender : MonoBehaviour, IRenderer
        {
            if (Sphere is RenderReference<TRender> sphere)
            {
                return sphere;
            }
            if (Circle is RenderReference<TRender> circle)
            {
                return circle;
            }
            throw new NotSupportedException($"{typeof(TRender)} is not supported.");
        }

        public IEnumerable<IRenderer> GetRenderers()
        {
            return (Sphere?.Renderers.Cast<IRenderer>() ?? Enumerable.Empty<IRenderer>())
                .Concat(Circle?.Renderers.Cast<IRenderer>() ?? Enumerable.Empty<IRenderer>());
        }

        private void UpdateReference<TRenderer>(RenderReference<TRenderer> renderReference)
            where TRenderer : MonoBehaviour, IRenderer
        {
            switch (renderReference)
            {
                case RenderReference<WireSphereRenderer> sphereRenderReference:
                    DynamicReferenceDestroy(Sphere, sphereRenderReference);
                    Sphere = sphereRenderReference;
                    return;
                case RenderReference<CircleRenderer> circleRenderReference:
                    DynamicReferenceDestroy(Circle, circleRenderReference);
                    Circle = circleRenderReference;
                    return;
                default:
                    throw new NotSupportedException($"{typeof(TRenderer)} is not supported.");
            }
        }

        public void UpdateSphereReference(RenderReference<WireSphereRenderer> sphere)
        {
            UpdateReference(sphere);
        }

        public void UpdateCircleReference(RenderReference<CircleRenderer> circle)
        {
            UpdateReference(circle);
        }

        public void DestroySphereReference()
        {
            Utils.Logger.LogDebug($"[PoiRenderReference][DestroySphereReference] destroying {Poi.Id} {Poi.Body}");
            Sphere?.DestroyImmediate();
            Sphere = null;
        }

        public void DestroyCircleReference()
        {
            Utils.Logger.LogDebug($"[PoiRenderReference][DestroySphereReference] destroying {Poi.Id} {Poi.Body}");
            Circle?.DestroyImmediate();
            Circle = null;
        }

        // TODO: maybe add a name so we can hunt these down if they become orphaned?
        private static GameObject CreateHolder()
        {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        public WireSphereRenderer CreateAndReplaceSphere()
        {
            Sphere ??= new RenderReference<WireSphereRenderer>(CreateHolder());
            return Sphere.NewRenderer(true);
        }

        public CircleRenderer CreateAndReplaceCircle()
        {
            Circle ??= new RenderReference<CircleRenderer>(CreateHolder());
            return Circle.NewRenderer(true);
        }

        public void DestroyImmediate()
        {
            Utils.Logger.LogDebug($"[PoiRenderReference][DestroyImmediate] destroying {Poi.Id} {Poi.Body}");
            DestroySphereReference();
            DestroyCircleReference();
        }
    }
}
