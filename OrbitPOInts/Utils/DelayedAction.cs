using System.Collections;

namespace OrbitPOInts.Utils
{
    // stateless
    public static class DelayedAction
    {
        public delegate void ActionToDelay();

        public static IEnumerator CreateCoroutine(ActionToDelay action, uint framesToDelay = 1)
        {
            for (var i = 0; i < framesToDelay; i++)
            {
                yield return null; // Wait for the next frame
            }

            action.Invoke();
        }
    }

    // instance based / resettable
    // TODO: determine if this will ever be needed/useful
    public class DelayedActionCoroutine : IEnumerator
    {
        public delegate void ActionToDelay();

        private readonly ActionToDelay _action;
        private readonly uint _framesToDelay;
        private uint _frameCount;

        public DelayedActionCoroutine(ActionToDelay action, uint framesToDelay)
        {
            _action = action;
            _framesToDelay = framesToDelay;
            _frameCount = 0;
        }

        public bool MoveNext()
        {
            if(_frameCount >= _framesToDelay)
            {
                _action.Invoke();
                return false; // Stops the coroutine
            }
            _frameCount++;
            return true; // Continues to the next iteration
        }

        public void Reset()
        {
            // Typically not used in Unity coroutines
            _frameCount = 0;
        }

        public object Current => null; // Yielded value, typically null for Unity coroutines waiting for the next frame

        public static IEnumerator Create(ActionToDelay action, uint framesToDelay = 1)
        {
            return new DelayedActionCoroutine(action, framesToDelay);
        }
    }
}
