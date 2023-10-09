#if TEST
using KSPMock;
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.Extensions.Unity
{
    public static class LineRendererExtensions
    {
        public static LineRenderer SetColor(this LineRenderer line, Color color)
        {
            line.startColor = color;
            line.endColor = color;
            return line;
        }

        public static LineRenderer SetWidth(this LineRenderer line, float width)
        {
            line.startWidth = width;
            line.endWidth = width;
            return line;
        }
    }
}
