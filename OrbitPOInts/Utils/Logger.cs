
using System;
using System.Collections;
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
                var value = Reflection.GetMemberValue(castedSender, args.PropertyName);
                var valueString = GetValueString(value);
                LogDebug($"{tag} {typeName}.{args.PropertyName}={valueString}");
            }
            else
            {
                LogDebug($"{tag} {typeName}.{args.PropertyName} (sender was not of type {typeName})");
            }
        }

        public static string GetValueString<T>(T value)
        {
            string HandleEnumerable(IEnumerable enumerable)
            {
                var countString = "?";
                // dont iterate unless LogDebugEnabled
                if (Settings.Instance.LogDebugEnabled)
                {
                    var count = enumerable.Cast<object>().Count();
                    countString = count.ToString();
                }

                return $"{enumerable.GetType()}[{countString}]";
            }

            return value switch
            {
                null => "null",
                IDictionary dictionary => $"{value.GetType()}[{dictionary.Count}]",
                ICollection collection => $"{value.GetType()}[{collection.Count}]",
                IEnumerable enumerable => HandleEnumerable(enumerable),
                _ => value.ToString()
            };
        }
    }
}
