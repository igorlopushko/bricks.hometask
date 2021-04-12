using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bricks.Hometask.Sandbox
{
    public class Server : IServer
    {
        private readonly object _lockObj = new object();
        
        private ConcurrentQueue<IRequest> _awatingRequests;
        private IList<IClient> _clients;
        private IList<int> _data;
        private bool _isRunning;
        private int _revision;

        public Server()
        {
            _clients = new List<IClient>();
            _data = new List<int>();
            _awatingRequests = new ConcurrentQueue<IRequest>();
            _revision = 0;
        }

        public void InitializeData()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            _isRunning = true;
            while (_isRunning)
            {
                // infinity loop to proceed data. 
                Console.WriteLine("Server is running");
                Thread.Sleep(Timeout.OneSecond);
            }
        }

        public void Stop()
        {
            _isRunning = false;

            // unsubscribe all clients
            foreach (var client in _clients)
            {
                client.OperationSent -= ReceiveClientOperationEventHandler;
            }
            _clients.Clear();
        }
        
        public void RegisterClient(IClient client)
        {
            _clients.Add(client);
            client.InitData(_data, _revision);
            client.OperationSent += ReceiveClientOperationEventHandler;
            
            // logging
            Console.WriteLine($"Register client with ID: '{client.ClientId}'");
        }

        public void UnregisterClient(IClient client)
        {
            _clients.RemoveAt(_clients.IndexOf(client));
            client.OperationSent -= ReceiveClientOperationEventHandler;
            
            // logging
            Console.WriteLine($"Unregister client with ID: '{client.ClientId}'");
        }

        private void ReceiveClientOperationEventHandler(Request request)
        {
            _awatingRequests.Enqueue(request);
            
            // logging
            StringBuilder str = new StringBuilder($"Operation received from Client ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            //str.Append($", type: '{operation.OperationType}'");
            //str.Append($", index: '{operation.Index}'");
            /*if (operation.OperationType == OperationType.Insert)
            {
                str.Append($", value: '{(operation.Value.HasValue ? operation.Value.Value : "NULL")}'");
            }*/
            Console.WriteLine(str.ToString());
        }
    }
}