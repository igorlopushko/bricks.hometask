using System;

namespace Bricks.Hometask.Base
{
    public class Operation : IOperation
    {
        public OperationType OperationType { get; }
        public int? Value { get; }
        public int Index { get; }
        public long Timestamp { get; }
        public int ClientId { get; }

        public Operation(int clientId, OperationType type, int index, int? value, long? timestamp = null)
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
}