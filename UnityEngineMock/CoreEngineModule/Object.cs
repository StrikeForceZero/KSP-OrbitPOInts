using System;

namespace UnityEngineMock
{
    public class Object
    {
        public string name { get; set; }

        public int GetInstanceID()
        {
            throw new NotImplementedException();
        }

        public static void Destroy(object obj)
        {

        }

        public static void DestroyImmediate(object obj)
        {

        }

        public static bool operator true(Object obj)
        {
            return obj != null;
        }

        public static bool operator false(Object obj)
        {
            return obj == null;
        }

        public static implicit operator bool(Object exists) => exists != null;
    }
}
