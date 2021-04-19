namespace Bricks.Hometask.Sandbox
{
    public class OperationFactory
    {
        public static IOperation CreateOperation(OperationType type, int index, int clientId, int? value = null, long? timestamp = null)
        {
            return new Operation(type, index, clientId, value, timestamp);
        }
    }
}
