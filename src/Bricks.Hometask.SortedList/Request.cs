using Bricks.Hometask.Base;
using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.SortedList
{
    public class Request<T> : IRequest<T>
    {
        private List<IOperation<T>> _operations;
        
        public IEnumerable<IOperation<T>> Operations
        {
            get
            {
                foreach (IOperation<T> item in _operations)
                {
                    yield return item;
                }
            }
        }

        public int ClientId { get; }
        public int Revision { get; }
        public bool IsAcknowledged { get; }

        public Request(int clientId, int revision,  IEnumerable<IOperation<T>> operations, bool isAcknowledged = false)
        {
            ClientId = clientId;
            Revision = revision;
            _operations = operations.ToList();
            IsAcknowledged = isAcknowledged;
        }
    }
}