using UnityEngine;

namespace OrbitPOInts
{
    [RequireComponent(typeof(LineRenderer))]
    public class WireSphereRenderer : MonoBehaviour
    {
        public float radius = 1.0f;
        public int latitudeLines = 10;
        public int longitudeLines = 10;
        private GameObject[] lineObjects = { };
        public Color wireframeColor = Color.green;
        public float lineWidth = 0.1f;

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
            lineObjects = new GameObject[latitudeLines + longitudeLines];

            // Draw latitude lines
            for (int i = 0; i < latitudeLines; i++)
            {
                float fraction = (float)i / (latitudeLines - 1);
                float angle = Mathf.Lerp(-Mathf.PI / 2, Mathf.PI / 2, fraction);
                float height = radius * Mathf.Sin(angle);
                float circleRadius = radius * Mathf.Cos(angle);

                DrawLatitudeLine(i, height, circleRadius);
            }

            // Draw longitude lines
            for (int i = 0; i < longitudeLines; i++)
            {
                float fraction = (float)i / longitudeLines;
                float angle = Mathf.Lerp(0, 2 * Mathf.PI, fraction);

                DrawLongitudeLine(i, angle);
            }

            foreach (var lineObject in lineObjects)
            {
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
        }

        void DrawLatitudeLine(int index, float height, float circleRadius)
        {
            GameObject lineObject = new GameObject($"LatitudeLine_{index}");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            // line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            line.material = MapView.fetch.orbitLinesMaterial;
            line.receiveShadows = false;
            // line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = false;
            line.positionCount = longitudeLines + 1;
            line.startColor = wireframeColor;
            line.endColor = wireframeColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            lineObject.transform.SetParent(transform);
            lineObject.transform.localPosition = Vector3.zero;

            for (int j = 0; j <= longitudeLines; j++)
            {
                float longFraction = (float)j / longitudeLines;
                float longAngle = Mathf.Lerp(0f, 360f, longFraction);
                Vector3 pos = new Vector3(
                    Mathf.Cos(longAngle * Mathf.Deg2Rad) * circleRadius,
                    height,
                    Mathf.Sin(longAngle * Mathf.Deg2Rad) * circleRadius
                );
                line.SetPosition(j, pos);
            }

            lineObjects[index] = lineObject;
        }

        void DrawLongitudeLine(int index, float angle)
        {
            GameObject lineObject = new GameObject($"LongitudeLine_{index}");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            // line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            line.material = MapView.fetch.orbitLinesMaterial;
            line.receiveShadows = false;
            // line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = false;
            line.positionCount = latitudeLines; // Making longitude lines conform to the curvature
            line.startColor = wireframeColor;
            line.endColor = wireframeColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            lineObject.transform.SetParent(transform);
            lineObject.transform.localPosition = Vector3.zero;

            for (int j = 0; j < latitudeLines; j++)
            {
                float latFraction = (float)j / (latitudeLines - 1);
                float latAngle = Mathf.Lerp(-Mathf.PI / 2, Mathf.PI / 2, latFraction);
                Vector3 pos = new Vector3(
                    radius * Mathf.Cos(latAngle) * Mathf.Cos(angle),
                    radius * Mathf.Sin(latAngle),
                    radius * Mathf.Cos(latAngle) * Mathf.Sin(angle)
                );
                line.SetPosition(j, pos);
            }

            lineObjects[latitudeLines + index] = lineObject;
        }

        private void OnDestroy()
        {
            foreach (GameObject lineObject in lineObjects)
            {
                Destroy(lineObject);
            }
        }

        public void SetEnabled(bool state)
        {
            enabled = state;
        }
    }
}