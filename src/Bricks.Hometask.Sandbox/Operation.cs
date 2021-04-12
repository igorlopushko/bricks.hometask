using System;
using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public class Operation : IOperation
    {
        public OperationType OperationType { get; private set; }
        public int? Value { get; private set; }
        public int Index { get; private set; }

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
        Delete
    }

    public class OperationSentEventArgs : EventArgs
    {
        public readonly IEnumerable<IOperation> Operations;
        public readonly int ClientId;
        public readonly int Revision;
        public readonly bool IsAcknowledged;

        public OperationSentEventArgs(int clientId, int revision,  IEnumerable<IOperation> operations, bool isAcknowledged = false)
        {
            ClientId = clientId;
            Revision = revision;
            Operations = operations;
            IsAcknowledged = isAcknowledged;
        }
    }
}