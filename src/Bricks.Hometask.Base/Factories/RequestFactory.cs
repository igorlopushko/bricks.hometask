using System.Collections.Generic;

namespace Bricks.Hometask.Base
{
    public class RequestFactory
    {
        public static IRequest CreateRequest(int clientId, int revision, IOperation operation, bool isAcknowledged = false)
        {
            return new Request(clientId, revision, operation, isAcknowledged);
        }
    }
}
