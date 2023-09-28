#if TEST
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts
{
    public class Logger
    {
        private static string TAG = "[OrbitPOInts]";

        public static void LogDebug(string str)
        {
            if (!Settings.LogDebugEnabled) return;
            Debug.Log($"{TAG}[{Time.frameCount}][DEBUG] {str}");
        }

        public static void Log(string str)
        {
            Debug.Log($"{TAG}[{Time.frameCount}][INFO] {str}");
        }

        public static void LogError(string str)
        {
            Debug.LogError($"{TAG}[{Time.frameCount}][ERROR] {str}");
        }
    }
}
