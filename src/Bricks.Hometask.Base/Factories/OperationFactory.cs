namespace Bricks.Hometask.Base
{
    public class OperationFactory
    {
        public static IOperation CreateOperation(int clientId, OperationType type, int index, int? value, long? timestamp = null)
        {
            return new Operation(clientId, type, index, value, timestamp);
        }
    }
}
