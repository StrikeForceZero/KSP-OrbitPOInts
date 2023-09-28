using System;

namespace UnityEngineMock
{
    public class GameObject : Object
    {
        public int layer = 0;
        public Transform transform;
        public GameObject gameObject;

        public extern Component GetComponent(Type type);
        public extern T GetComponent<T>();

        public extern T[] GetComponents<T>();

        public extern Component AddComponent(Type type);

        public extern T AddComponent<T>();

        public GameObject() {}

        public GameObject(string name)
        {
            this.name = name;
        }

        public static GameObject CreatePrimitive(PrimitiveType primitiveType)
        {
            throw new NotImplementedException();
        }

        public static extern GameObject Find(string name);

        public static extern T[] FindObjectsOfType<T>();
    }
}
