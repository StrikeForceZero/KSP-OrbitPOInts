using System.Collections;

namespace OrbitPOInts.Utils
{
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
}
