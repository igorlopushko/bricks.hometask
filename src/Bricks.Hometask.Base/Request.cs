using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.Base
{
    public class Request : IRequest
    {
        private List<IOperation> _operations;
        
        public IEnumerable<IOperation> Operations
        {
            get
            {
                foreach (IOperation item in _operations)
                {
                    yield return item;
                }
            }
        }

        public int ClientId { get; }
        public int Revision { get; }
        public bool IsAcknowledged { get; }

        public Request(int clientId, int revision,  IEnumerable<IOperation> operations, bool isAcknowledged = false)
        {
            ClientId = clientId;
            Revision = revision;
            _operations = operations.ToList();
            IsAcknowledged = isAcknowledged;
        }
    }
}