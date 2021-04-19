using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public delegate void BroadcastEventHandler<T>(IRequest<T> request);

    public interface IServer<T>
    {
        /// <summary>Gets current server state.</summary>
        public IEnumerable<T> Data { get; }
        
        /// <summary>Gets current server revision number.</summary>
        public int Revision { get; }

        /// <summary>Runs server.</summary>
        public void Run();
        
        /// <summary>Stops server's execution.</summary>
        public void Stop();
        
        /// <summary>Registers a new client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void RegisterClient(IClient<T> client);
        
        /// <summary>Unregisters a client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void UnregisterClient(IClient<T> client);


        /// <summary>An event that occurs when the server emits a new request.</summary>
        public event BroadcastEventHandler<T> BroadcastRequest;
    }
}