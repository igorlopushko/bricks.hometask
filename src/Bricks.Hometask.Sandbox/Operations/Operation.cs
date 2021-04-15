using System;

namespace Bricks.Hometask.Sandbox
{
    public class Operation : IOperation
    {
        public OperationType OperationType { get; }
        public int? Value { get; }
        public int Index { get; }
        public long Timestamp { get; }

        public Operation(OperationType type, int index, int? value = null, long? timestamp = null)
        {
            OperationType = type;
            Index = index;
            Value = value;
            
            if (timestamp.HasValue)
            {
                // re-apply timestamp if specified.
                Timestamp = timestamp.Value;
            }
            else
            {
                // generate new timestamp if not specified.
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            }
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || this.GetType() != obj.GetType())
            {
                return false;
            }
            else 
            {
                Operation p = (Operation) obj;
                return Index == p.Index;
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