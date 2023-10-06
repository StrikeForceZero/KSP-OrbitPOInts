#if TEST
using KSPMock;
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts
{
    public interface IRenderer
    {

        public string name { get; set; }
        public float radius { get; set; }
        public Color wireframeColor { get; set; }
        public float lineWidth { get; set; }
        public bool IsDying { get; }
        public string groupId { get; }
        public void SetEnabled(bool state);
        public void SetColor(Color color);
        public void SetWidth(float width);

        public bool Equals(object obj);

        public int GetHashCode();
    }
}
