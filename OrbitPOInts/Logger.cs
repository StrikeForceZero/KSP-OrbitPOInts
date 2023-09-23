using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OrbitPOInts
{
    public class Logger
    {
        private static string TAG = "[OrbitPOInts]";

        public static void Log(string str)
        {
            Debug.Log($"{TAG}[{Time.frameCount}] {str}");
        }

        public static void LogError(string str)
        {
            Debug.LogError($"{TAG}[{Time.frameCount}] {str}");
        }
    }
}
