namespace Bricks.Hometask.Sandbox
{
    public class Operation : IOperation
    {
        public OperationType OperationType { get; }
        public int? Value { get; }
        public int Index { get; }

        public Operation(OperationType type, int index, int? value = null)
        {
            OperationType = type;
            Index = index;
            Value = value;
        }
    }

    public enum OperationType
    {
        Insert,
        Update,
        Delete
    }
}