
using System.ComponentModel;
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

        public static void LogPropertyChange<T>(object sender, PropertyChangedEventArgs args, string tag = "[PropertyChange]")
        {
            var typeName = typeof(T).Name;

            if (sender is T castedSender)
            {
                LogDebug($"{tag} {typeName}.{args.PropertyName}={Reflection.AccessProp(castedSender, args.PropertyName)}");
            }
            else
            {
                LogDebug($"{tag} {typeName}.{args.PropertyName} (sender was not of type {typeName})");
            }
        }
    }
}
