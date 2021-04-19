namespace Bricks.Hometask.OperationTransformation
{
    public class OperationFactory<T>
    {
        public static IOperation<T> CreateOperation(OperationType type, int index, int clientId, T value, long? timestamp = null)
        {
            return new Operation<T>(type, index, clientId, value, timestamp);
        }
    }
}
