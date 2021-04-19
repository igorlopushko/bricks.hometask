using System;

namespace Bricks.Hometask.OperationTransformation
{
    public static class RandomGenerator
    {
        private static readonly Random GetRandom = new Random();
        
        /// <summary>Gets random integer number in specified range.</summary>
        /// <param name="min">Min possible value.</param>
        /// <param name="max">Max possible value.</param>
        /// <returns>Random integer value.</returns>
        public static int GetNumber(int min, int max)
        {
            lock(GetRandom)
            {
                return GetRandom.Next(min, max);
            }
        }
        
        /// <summary>Gets random operation type.</summary>
        /// <returns>Randomly generated operation.</returns>
        public static OperationType GetOperation()
        {
            int value = GetNumber(0, 3);
            switch (value)
            {
                case 0:
                    return OperationType.Insert;
                case 1:
                    return OperationType.Update;
                case 2:
                    return OperationType.Delete;
                default:
                    return OperationType.Insert;
            }
        }

        /// <summary>Gets random index value starting from 0 to specified upper boundary.</summary>
        /// <param name="maxBound">Index possible upper boundary.</param>
        /// <returns>Randomly generated index number.</returns>
        public static int GetIndex(int maxBound)
        {
            return GetNumber(0, maxBound);
        }
    }
}