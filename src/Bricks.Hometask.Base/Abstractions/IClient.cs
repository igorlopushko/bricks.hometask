using System.Collections.Generic;

namespace Bricks.Hometask.Base
{
    public delegate void RequestSentEventHandler<T>(IRequest<T> request);

    public interface IClient<T>
    {
        /// <summary>Gets client unique identifier.</summary>
        public int ClientId { get; }

        /// <summary>Gets current client state.</summary>
        public IEnumerable<T> Data { get; }
        
        /// <summary>Runs client.</summary>
        public void Run();

        /// <summary>Stops client's execution.</summary>
        public void Stop();
        
        /// <summary>Sync data with the source of truth.</summary>
        /// <param name="data">Data to be synced.</param>
        /// <param name="revision">Last synced revision number.</param>
        public void SyncData(IEnumerable<T> data, int revision);
        
        /// <summary>Receives operation from external world.</summary>
        /// <param name="operation">Operation instance to be processed.</param>
        public void PushOperation(IOperation<T> operation);
        
        /// <summary>An event that occurs when a new request is sent to the server.</summary>
        public event RequestSentEventHandler<T> RequestSent;
    }
}