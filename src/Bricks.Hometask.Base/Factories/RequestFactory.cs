using System.Collections.Generic;

namespace Bricks.Hometask.Base
{
    public class RequestFactory<T>
    {
        public static IRequest<T> CreateRequest(int clientId, int revision, IEnumerable<IOperation<T>> operations, bool isAcknowledged = false)
        {
            return new Request<T>(clientId, revision, operations, isAcknowledged);
        }
    }
}
