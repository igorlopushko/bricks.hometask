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
                    return new Operation(OperationType.Update, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
                case OperationType.Delete:
                    return new Operation(OperationType.Delete, index);
                default:
                    return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
            }
        }
    }
}