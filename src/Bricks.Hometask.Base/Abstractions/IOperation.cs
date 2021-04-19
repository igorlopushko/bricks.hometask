namespace Bricks.Hometask.Base
{
    public interface IOperation<T>
    {
        /// <summary>Gets operation type.</summary>
        public OperationType OperationType { get; }
        
        /// <summary>Gets operation value.</summary>
        public T Value { get; }
        
        /// <summary>Gets operation index.</summary>
        public int Index { get; }
        
        /// <summary>Gets operation timestamp.</summary>
        public long Timestamp { get; }

        /// <summary>Gets Client unique identifier.</summary>
        public int ClientId { get; }
    }
}