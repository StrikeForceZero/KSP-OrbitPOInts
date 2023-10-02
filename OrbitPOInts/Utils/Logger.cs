#if TEST
using UnityEngineMock;

#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.Utils
{
    public class Logger
    {
        private static string TAG = "[OrbitPOInts]";

        public static void LogDebug(string str)
        {
            if (!Settings.Instance.LogDebugEnabled) return;
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
