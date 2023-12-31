using OrbitPOInts.Extensions.Unity;

#if TEST
using KSPMock;
using UnityEngineMock;
using System.Linq;
using KSP_MapView = KSPMock.MapView;
using KSP_HighLogic = KSPMock.HighLogic;
using KSP_GameScenes = KSPMock.GameScenes;
#else
using UniLinq;
using UnityEngine;
using KSP_MapView = MapView;
using KSP_HighLogic = HighLogic;
using KSP_GameScenes = GameScenes;
#endif
using OrbitPOInts.Extensions.Unity;

namespace OrbitPOInts
{
    using MapView = KSP_MapView;
    using HighLogic = KSP_HighLogic;
    using GameScenes = KSP_GameScenes;

    [RequireComponent(typeof(LineRenderer))]
    public class CircleRenderer : MonoBehaviour, IRenderer
    {
        public float radius { get; set; } = 1.0f;
        public Color wireframeColor { get; set; } = Color.green;
        public float lineWidth { get; set; } = 0.1f;
        public int segments { get; set; } = 50;
        private GameObject lineObject;
        public bool IsDying { get; private set; }

        private void Awake()
        {
            enabled = false;

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                gameObject.layer = 10;
            }
            else
            {
                gameObject.layer = 24;
            }
        }

        private Material GetMaterial()
        {
            var mapViewReady = MapView.fetch != null;
            return mapViewReady ? MapView.fetch.orbitLinesMaterial : new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }

        void Start()
        {
            lineObject = new GameObject(NameKey);
            var line = lineObject.AddComponent<LineRenderer>();
            line.material = GetMaterial();
            line.receiveShadows = false;
            line.useWorldSpace = false;
            line.positionCount = segments + 1; // +1 to close the circle
            line
                .SetColor(wireframeColor)
                .SetWidth(lineWidth);
            lineObject.transform.SetParent(transform);
            lineObject.transform.localPosition = Vector3.zero;

            for (var i = 0; i <= segments; i++)
            {
                var angle = Mathf.Deg2Rad * (i * 360f / segments);
                var x = radius * Mathf.Cos(angle);
                var z = radius * Mathf.Sin(angle);

                line.SetPosition(i, new Vector3(x, 0, z));
            }

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                gameObject.layer = 10;
                lineObject.gameObject.layer = 10;
            }
            else
            {
                gameObject.layer = 24;
                lineObject.gameObject.layer = 24;
            }
        }

        private void OnDestroy()
        {
            IsDying = true;
            DestroyImmediate(lineObject);
        }

        public void SetEnabled(bool state)
        {
            enabled = state;
            if (!lineObject.IsAlive()) return;
            lineObject.SetActive(state);
        }

        public void SetColor(Color color)
        {
            wireframeColor = color;
            if (!lineObject.IsAlive()) return;
            foreach (var line in lineObject.GetComponents<LineRenderer>())
            {
                line.SetColor(color);
            }
        }

        public void SetWidth(float width)
        {
            lineWidth = width;
            if (!lineObject.IsAlive()) return;
            foreach (var line in lineObject.GetComponents<LineRenderer>())
            {
                line.SetWidth(width);
            }
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public static string NameKey => "CircleLine";

        public override bool Equals(object obj)
        {
            if (obj is CircleRenderer other)
            {
                return GetInstanceID() == other.GetInstanceID();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GetInstanceID();
        }
    }
}
