using System;

namespace Bricks.Hometask.Sandbox
{
    public static class OperationRandomGenerator
    {
        public static IOperation GenerateRandomOperation(int collectionCount)
        {
            OperationType type = RandomGenerator.GetOperation();
            int index = RandomGenerator.GetIndex(collectionCount);

            switch (type)
            {
                case OperationType.Insert:
                    return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
                case OperationType.Update:
                    if (collectionCount == 0)
                    {
                        return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
                    }
                    return new Operation(OperationType.Update, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
                case OperationType.Delete:
                    if (collectionCount == 0)
                    {
                        return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
                    }
                    return new Operation(OperationType.Delete, index);
                default:
                    throw new ArgumentOutOfRangeException($"Operation {type} is not supported");
            }
        }
    }
}