using System;

namespace Bricks.Hometask.OperationTransformation
{
    public class Operation<T> : IOperation<T>
    {
        public OperationType OperationType { get; }
        public T Value { get; }
        public int Index { get; }
        public long Timestamp { get; }
        public int ClientId { get; }

        public Operation(OperationType type, int index, int clientId, T value, long? timestamp = null)
        {
            OperationType = type;
            Index = index;
            Value = value;
            ClientId = clientId;
            
            if (timestamp.HasValue)
            {
                // re-apply timestamp if specified.
                Timestamp = timestamp.Value;
            }
            else
            {
                // generate new timestamp if not specified.
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            }
        }
    }

    public enum OperationType
    {
        Insert,
        Update,
        Delete
    }
}