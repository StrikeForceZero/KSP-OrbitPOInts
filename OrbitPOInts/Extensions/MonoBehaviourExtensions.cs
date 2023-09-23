using UnityEngine;

namespace OrbitPOInts.Extensions
{
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Determines whether the specified MonoBehaviour is not destroyed.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified MonoBehaviour is not destroyed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method utilizes Unity's implicit boolean conversion for UnityEngine.Object-derived types,
        /// such as MonoBehaviour, which evaluates to <c>false</c> in a boolean context if the object is destroyed.
        /// However, it does not account for components that are marked for destruction but not yet destroyed
        /// within the same frame. The actual destruction occurs at the end of the frame.
        /// </remarks>
        public static bool IsAlive(this MonoBehaviour monoBehaviour)
        {
            return monoBehaviour;
        }
    }
}
