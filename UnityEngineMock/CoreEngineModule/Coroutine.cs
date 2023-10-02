using System;

namespace UnityEngineMock
{
    public sealed class Coroutine : YieldInstruction
    {
        internal IntPtr Ptr;

        private Coroutine()
        {
        }

        ~Coroutine() => Coroutine.ReleaseCoroutine(this.Ptr);

        private static extern void ReleaseCoroutine(IntPtr ptr);
    }
}
