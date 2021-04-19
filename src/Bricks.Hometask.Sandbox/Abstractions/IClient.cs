using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public delegate void OperationSentEventHandler(Request request);
    
    public interface IClient
    {
        /// <summary>Gets client unique identifier.</summary>
        public int ClientId { get; }

        /// <summary>Gets current client state.</summary>
        public IEnumerable<int> Data { get; }
        
        /// <summary>Runs client.</summary>
        public void Run();

        /// <summary>Stops client's execution.</summary>
        public void Stop();
        
        /// <summary>Sync data with the source of truth.</summary>
        /// <param name="data">Data to be synced.</param>
        /// <param name="revision">Last synced revision number.</param>
        public void SyncData(IEnumerable<int> data, int revision);
        
        /// <summary>Receives request from server to sync the state.</summary>
        /// <param name="request">Request operation entity.</param>
        public void ReceiveRequestFromServer(IRequest request);
        
        /// <summary>Receives operation from external world.</summary>
        /// <param name="operation">Operation instance to be processed.</param>
        public void PushOperation(IOperation operation);
        
        /// <summary>An event that occurs when operation is sent to the server.</summary>
        public event OperationSentEventHandler OperationSent;
    }
}