using System.Collections.Generic;

namespace Bricks.Hometask.Base
{
    public delegate void BroadcastEventHandler(IRequest request);

    public interface IServer
    {
        /// <summary>Gets current server state.</summary>
        public IEnumerable<int> Data { get; }
        
        /// <summary>Gets current server revision number.</summary>
        public int Revision { get; }

        /// <summary>Gets server revision log.</summary>
        public IDictionary<int, IList<IOperation>> RevisionLog { get; }

        /// <summary>Runs server.</summary>
        public void Run();
        
        /// <summary>Stops server's execution.</summary>
        public void Stop();
        
        /// <summary>Connects a new client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void ConnectClient(IClient client);

        /// <summary>Reconnects a client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void ReconnectClient(IClient client);

        /// <summary>Disconnects a client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void DisconnectClient(IClient client);


        /// <summary>An event that occurs when the server emits a new request.</summary>
        public event BroadcastEventHandler BroadcastRequest;
    }
}