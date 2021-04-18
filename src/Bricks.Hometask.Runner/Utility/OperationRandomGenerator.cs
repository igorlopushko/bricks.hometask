using System;

namespace Bricks.Hometask
{
    public static class OperationRandomGenerator
    {
        public static IOperation GenerateRandomOperation(int collectionCount)
        {
            OperationType type = RandomGenerator.GetOperation();
            int index = RandomGenerator.GetIndex(collectionCount);
            int minValue = 0;
            int maxValue = 10;

            switch (type)
            {
                case OperationType.Insert:
                    return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Update:
                    if (collectionCount == 0)
                    {
                        return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Update, index, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Delete:
                    if (collectionCount == 0)
                    {
                        return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Delete, index);
                default:
                    throw new ArgumentOutOfRangeException($"Operation {type} is not supported");
            }
        }
    }
}