using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.Base
{
    public class Request : IRequest
    {        
        public IOperation Operation { get; }
        public int ClientId { get; }
        public int Revision { get; }
        public bool IsAcknowledged { get; }

        public Request(int clientId, int revision,  IOperation operation, bool isAcknowledged = false)
        {
            ClientId = clientId;
            Revision = revision;
            Operation = operation;
            IsAcknowledged = isAcknowledged;
        }
    }
}