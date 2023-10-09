namespace OrbitPOInts.Extensions
{
    public static class IRendererExtensions
    {
        public static bool IsAliveAndEnabled(this IRenderer renderer)
        {
            // ReSharper disable once MergeIntoPattern
            return renderer != null && !renderer.IsDying && renderer.enabled;
        }
    }
}
