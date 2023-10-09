using System;
using OrbitPOInts.Extensions.Unity;
#if TEST
using UnityEngineMock;
using JB_Annotations = UnityEngineMock.JetBrains.Annotations;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using JB_Annotations = JetBrains.Annotations;
#endif

namespace OrbitPOInts
{
    public class RenderReference<TRenderer> where TRenderer : MonoBehaviour, IRenderer
    {
        public GameObject Root { get; private set; }
        public TRenderer[] Renderers => Root.GetComponents<TRenderer>();

        public RenderReference(GameObject root)
        {
            Utils.Logger.LogDebug($"[RenderReference][Constructor] {typeof(TRenderer).Name} {root.GetInstanceID()}");
            Root = root;
        }

        public TRenderer NewRenderer(bool destroyOld = false)
        {
            Utils.Logger.LogDebug($"[RenderReference][NewRenderer] {typeof(TRenderer).Name}");
            if (destroyOld) RenderersDestroyImmediate();
            var renderer = Root.AddComponent<TRenderer>();
            Utils.Logger.LogDebug($"[RenderReference][NewRenderer] {typeof(TRenderer).Name} {renderer.GetInstanceID()}");
            Utils.Logger.LogDebug($"[RenderReference][NewRenderer] {typeof(TRenderer).Name}:{Renderers.Length} {renderer.GetInstanceID()}");
            return renderer;
        }

        public void RenderersDestroyImmediate()
        {
            Utils.Logger.LogDebug($"[RenderReference][RenderersDestroyImmediate] {typeof(TRenderer).Name}:{Renderers.Length}");
            foreach (var renderer in Renderers)
            {
                Utils.Logger.LogDebug($"[RenderReference][RenderersDestroyImmediate] {typeof(TRenderer).Name} {renderer.GetInstanceID()}");
                renderer.DestroyImmediateIfAlive();
            }
        }

        public void DestroyImmediate()
        {
            RenderersDestroyImmediate();
            Utils.Logger.LogDebug($"[RenderReference][DestroyImmediate] {typeof(TRenderer).Name} {Root.GetInstanceID()}");
            Root.DestroyImmediateIfAlive();
        }

        public void SetEnabled(bool state)
        {
            Utils.Logger.LogDebug($"[RenderReference][SetEnabled] {state} {typeof(TRenderer).Name} {Renderers.Length}");
            foreach (var renderer in Renderers)
            {
                Utils.Logger.LogDebug($"[RenderReference][SetEnabled] {state} {typeof(TRenderer).Name} {renderer.GetInstanceID()}");
                renderer.enabled = state;
            }
        }
    }
}
