using System;
using System.Collections;

namespace UnityEngineMock
{
    public class MonoBehaviour : Behaviour
    {
        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                throw new NullReferenceException("routine is null");
            if (!MonoBehaviour.IsObjectMonoBehaviour((Object) this))
                throw new ArgumentException("Coroutines can only be stopped on a MonoBehaviour");
            return this.StartCoroutineManaged2(routine);
        }

        private extern Coroutine StartCoroutineManaged2(IEnumerator enumerator);

        private static bool IsObjectMonoBehaviour(Object obj)
        {
            throw new NotImplementedException();
        }
    }
}
