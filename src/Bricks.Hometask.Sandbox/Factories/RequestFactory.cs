using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public class RequestFactory
    {
        public static IRequest CreateRequest(int clientId, int revision, IEnumerable<IOperation> operations, bool isAcknowledged = false)
        {
            return new Request(clientId, revision, operations, isAcknowledged);
        }
    }
}
