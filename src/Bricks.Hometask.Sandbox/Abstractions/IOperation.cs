namespace Bricks.Hometask.Sandbox
{
    public interface IOperation
    {
        /// <summary>Gets operation type.</summary>
        public OperationType OperationType { get; }
        
        /// <summary>Gets operation value.</summary>
        public int? Value { get; }
        
        /// <summary>Gets operation index.</summary>
        public int Index { get; }
        
        /// <summary>Gets operation timestamp.</summary>
        public long Timestamp { get; }
    }
}