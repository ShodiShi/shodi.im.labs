using System;

namespace lab6
{
    public static class RandomGenerator
    {
        private static long M = 2147483647;
        private static long Beta = 16807;
        private static long X = Environment.TickCount;

        public static double NextDouble()
        {
            X = (Beta * X) % M;
            return (double)X / M;
        }
    }
}
