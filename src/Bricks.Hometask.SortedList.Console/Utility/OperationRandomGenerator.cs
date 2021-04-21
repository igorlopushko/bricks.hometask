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
                    return new Operation(client.ClientId, OperationType.Insert, -1, RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Update:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(client.ClientId, OperationType.Insert, -1, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(client.ClientId, OperationType.Update, GetIndex(client), RandomGenerator.GetNumber(minValue, maxValue));
                case OperationType.Delete:
                    if (client.Data.Count() == 0)
                    {
                        return new Operation(client.ClientId, OperationType.Insert, -1, RandomGenerator.GetNumber(minValue, maxValue));
                    }
                    return new Operation(client.ClientId, OperationType.Delete, GetIndex(client), -1);
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