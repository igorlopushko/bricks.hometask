using Bricks.Hometask.Base;
using Bricks.Hometask.Utility;
using System;
using System.Linq;

namespace Bricks.Hometask.SortedList.Console
{
    public static class OperationRandomGenerator
    {
        public static IOperation GenerateRandomOperation(IClient client)
        {
            OperationType type = RandomGenerator.GetOperation();

            int minValue = 11;
            int maxValue = 99;

            switch (type)
            {
                case OperationType.Insert:                    
                    return new Operation(OperationType.Insert, -1, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Update:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(OperationType.Insert, -1, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Update, GetIndex(client), client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Delete:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(OperationType.Insert, -1, client.ClientId, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(OperationType.Delete, GetIndex(client), client.ClientId, -1);
                default:
                    throw new ArgumentOutOfRangeException($"Operation {type} is not supported");
            }
        }

        private static int GetIndex(IClient client)
        {
            return RandomGenerator.GetIndex(client.Data.Count() == 0 ? 0 : client.Data.Count() - 1);
        }
    }
}