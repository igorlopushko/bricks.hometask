using System;

namespace Bricks.Hometask.Sandbox
{
    public static class OperationRandomGenerator
    {
        public static IOperation GenerateRandomOperation(int collectionCount, int clientId)
        {
            OperationType type = RandomGenerator.GetOperation();
            int index = RandomGenerator.GetIndex(collectionCount);

            if (collectionCount > 0 && index >= collectionCount)
            {
                throw new Exception();
            }

            int minValue = 0;
            int maxValue = 10;

            switch (type)
            {
                case OperationType.Insert:
                    return new Operation(OperationType.Insert, index, clientId, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Update:
                    if (collectionCount == 0)
                    {
                        return new Operation(OperationType.Insert, index, clientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Update, index, clientId, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Delete:
                    if (collectionCount == 0)
                    {
                        return new Operation(OperationType.Insert, index, clientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Delete, index, clientId);
                default:
                    throw new ArgumentOutOfRangeException($"Operation {type} is not supported");
            }
        }
    }
}