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
            Root = root;
        }

        public TRenderer NewRenderer(bool destroyOld = false)
        {
            if (destroyOld) RenderersDestroyImmediate();
            return Root.AddComponent<TRenderer>();
        }

        public void RenderersDestroyImmediate()
        {
            foreach (var renderer in Renderers)
            {
                renderer.DestroyImmediateIfAlive();
            }
        }

        public void DestroyImmediate()
        {
            RenderersDestroyImmediate();
            Root.DestroyImmediateIfAlive();
        }

        public void SetEnabled(bool state)
        {
            foreach (var renderer in Renderers)
            {
                renderer.enabled = state;
            }
        }
    }
}