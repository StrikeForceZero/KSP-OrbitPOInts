using UnityEngine;

namespace OrbitPOInts
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircleRenderer : MonoBehaviour
    {
        public float radius = 1.0f;
        public Color wireframeColor = Color.green;
        public float lineWidth = 0.1f;
        public int segments = 50;
        private GameObject lineObject;

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

        void Start()
        {
            lineObject = new GameObject($"CircleLine");
            var line = lineObject.AddComponent<LineRenderer>();
            line.material = MapView.fetch.orbitLinesMaterial;
            line.receiveShadows = false;
            line.useWorldSpace = false;
            line.positionCount = segments + 1; // +1 to close the circle
            line.startColor = wireframeColor;
            line.endColor = wireframeColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            lineObject.transform.SetParent(transform);
            lineObject.transform.localPosition = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * (i * 360f / segments);
                float x = radius * Mathf.Cos(angle);
                float z = radius * Mathf.Sin(angle);

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
            Destroy(lineObject);
        }

        public void SetEnabled(bool state)
        {
            enabled = state;
        }
    }
}