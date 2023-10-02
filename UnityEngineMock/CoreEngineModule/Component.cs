using System;

namespace UnityEngineMock
{
    public class Component : Object
    {
        public Transform transform = new Transform();

        public GameObject gameObject = new GameObject();

        public Component GetComponent(Type type) => this.gameObject.GetComponent(type);

        public T GetComponent<T>() where T : Component
        {
            return (T)GetComponent(typeof(T));
        }
    }
}
