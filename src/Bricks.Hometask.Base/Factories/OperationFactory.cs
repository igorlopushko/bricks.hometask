namespace Bricks.Hometask.Base
{
    public class OperationFactory
    {
        public static IOperation CreateOperation(OperationType type, int index, int clientId, int value, long? timestamp = null)
        {
            return new Operation(type, index, clientId, value, timestamp);
        }
    }
}
