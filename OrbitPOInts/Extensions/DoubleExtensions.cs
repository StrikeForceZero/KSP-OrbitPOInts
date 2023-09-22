using System;

namespace OrbitPOInts.Extensions
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Determines whether two double values are approximately equal, using absolute comparison.
        /// </summary>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <param name="epsilon">The absolute tolerance level. Defaults to 1e-6.</param>
        /// <returns>True if the absolute difference between the values is less than epsilon; otherwise, false.</returns>
        public static bool AreEqual(this double a, double b, double epsilon = 1e-6)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Determines whether two double values are approximately equal, using relative comparison.
        /// </summary>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <param name="relativeEpsilon">The relative tolerance level. Defaults to 1e-6.</param>
        /// <returns>True if the relative difference between the values is less than relativeEpsilon; otherwise, false.</returns>
        public static bool AreRelativelyEqual(this double a, double b, double relativeEpsilon = 1e-6)
        {
            var max = Math.Max(Math.Abs(a), Math.Abs(b));
            return max == 0 || Math.Abs(a - b) / max < relativeEpsilon;
        }
    }
}