using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public interface IRequest
    {
        public IEnumerable<IOperation> Operations { get; }
        public int ClientId { get; }
        public int Revision { get; }
        public bool IsAcknowledged { get; }
    }
}