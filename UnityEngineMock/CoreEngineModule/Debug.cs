using System;

namespace UnityEngineMock
{
    public class Debug
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            Console.WriteLine(message);
        }
        
        public static void LogWarning(string message)
        {
            Console.WriteLine(message);
        }
    }
}
