namespace Bricks.Hometask.Sandbox
{
    public interface IOperation
    {
        public OperationType OperationType { get; }
        public int? Value { get; }
        public int Index { get; }
    }
}