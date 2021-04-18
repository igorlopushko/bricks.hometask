using System;
using System.Linq;

namespace Bricks.Hometask.Sandbox
{
    public static class OperationRandomGenerator
    {
        public static IOperation GenerateRandomOperation(IClient client)
        {
            OperationType type = RandomGenerator.GetOperation();
            int index = RandomGenerator.GetIndex(client.Data.Count());

            int minValue = 0;
            int maxValue = 10;

            switch (type)
            {
                case OperationType.Insert:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(OperationType.Insert, 0, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Insert, index, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Update:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(OperationType.Insert, 0, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Update, index, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Delete:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(OperationType.Insert, 0, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Delete, index, client.ClientId);
                default:
                    throw new ArgumentOutOfRangeException($"Operation {type} is not supported");
            }
        }
    }
}