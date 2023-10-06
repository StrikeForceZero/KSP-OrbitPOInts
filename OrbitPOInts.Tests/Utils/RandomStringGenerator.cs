using System;
using System.Linq;

namespace OrbitPOInts.Tests.Utils
{
    public static class RandomStringGenerator
    {
        private static readonly Random _random = new Random();

        public static string Generate(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
