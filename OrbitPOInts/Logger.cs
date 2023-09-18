using Debug = UnityEngine.Debug;

namespace OrbitPOInts
{
    public class Logger
    {
        private static string TAG = "[OrbitPOInts]";
        public static void Log(string str)
        {
            Debug.Log($"{TAG} {str}");
        }    
        public static void LogError(string str)
        {
            Debug.LogError($"{TAG} {str}");
        }
    }
}