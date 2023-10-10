#if TEST
using UnityEngineMock;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
#endif

namespace OrbitPOInts.Extensions.Unity
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Determines whether the specified GameObject is not destroyed.
        /// </summary>
        /// <param name="gameObject">The GameObject to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified GameObject is not destroyed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method utilizes Unity's implicit boolean conversion for UnityEngine.Object-derived types,
        /// such as GameObject. In Unity, when a UnityEngine.Object-derived type is destroyed, it evaluates
        /// to <c>false</c> in a boolean context. The actual destruction of the GameObject occurs at the end
        /// of the frame.
        /// </remarks>
        public static bool IsAlive(this GameObject gameObject)
        {
            return gameObject;
        }

        public static void DestroyImmediateIfAlive(this GameObject gameObject)
        {
            if (gameObject.IsAlive())
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
