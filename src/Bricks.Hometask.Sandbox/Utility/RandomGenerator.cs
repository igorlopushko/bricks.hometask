using System;

namespace Bricks.Hometask.Sandbox
{
    public class RandomGenerator
    {
        private static readonly Random GetRandom = new Random();

        public static int GetNumber(int min, int max)
        {
            lock(GetRandom)
            {
                return GetRandom.Next(min, max);
            }
        }
        
        public static OperationType GetOperation()
        {
            int value = RandomGenerator.GetNumber(0, 2);
            return value == 0 ? OperationType.Insert : OperationType.Delete;
        }

        public static int GetIndex(int maxBound)
        {
            return GetNumber(0, maxBound);
        }
    }
}