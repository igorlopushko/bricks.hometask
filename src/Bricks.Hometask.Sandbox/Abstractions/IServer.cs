using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public interface IServer
    {
        /// <summary>Gets current server state.</summary>
        public IEnumerable<int> Data { get; }
        
        /// <summary>Gets current server revision number.</summary>
        public int Revision { get; }
        
        /// <summary>Gets boolean value which determines whether the server is alive or not.</summary>
        public bool IsAlive { get; }

        /// <summary>Runs server.</summary>
        public void Run();
        
        /// <summary>Stops server's execution.</summary>
        public void Stop();
        
        /// <summary>Registers a new client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void RegisterClient(IClient client);
        
        /// <summary>Unregisters a client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void UnregisterClient(IClient client);
    }
}