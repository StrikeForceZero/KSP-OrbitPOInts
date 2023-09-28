namespace UnityEngineMock
{
    public class LineRenderer : GameObject
    {
        public Material material;
        public bool receiveShadows;
        public bool useWorldSpace;
        public int positionCount;
        public Color startColor;
        public Color endColor;
        public float startWidth;
        public float endWidth;

        public void SetPosition(int index, Vector3 position)
        {

        }
    }
}
