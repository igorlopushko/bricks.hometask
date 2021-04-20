using Bricks.Hometask.Base;
using Bricks.Hometask.Utility;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bricks.Hometask.SortedList.Console
{
    public class Server : IServer
    {
        private readonly object _locker = new object();

        private readonly System.ConsoleColor _color = System.ConsoleColor.Red;
        private readonly ConsoleLogger _logger;

        private readonly ConcurrentQueue<IRequest> _awaitingRequests;
        private readonly ConcurrentDictionary<int, List<IOperation>> _revisionLog;
        private readonly ConcurrentDictionary<int, IClient> _clients;
        private readonly IList<int> _data;
        private int _revision;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private readonly bool _logginEnabled;
                
        public IEnumerable<int> Data
        {
            get
            {
                lock(_locker)
                {
                    foreach(int item in _data)
                    {
                        yield return item;
                    }
                }
            }
        }

        public int Revision
        {
            get
            {
                lock(_locker)
                {
                    return _revision;
                }
            }
        }

        public event BroadcastEventHandler BroadcastRequest;

        /// <summary>Constructor.</summary>
        public Server(IConfigurationRoot configuration)
        {
            _clients = new ConcurrentDictionary<int, IClient>();
            _data = new List<int>();
            _awaitingRequests = new ConcurrentQueue<IRequest>();
            _revisionLog = new ConcurrentDictionary<int, List<IOperation>>();
            _revision = 0;
            _logger = new ConsoleLogger(_color);

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            _logginEnabled = bool.Parse(configuration["LoggingEnabled"]);
        }

        public void Initialize(IList<int> data)
        {
            lock(_locker)
            {
                _data.Clear();
                foreach (int item in data)
                {
                    _data.Add(item);
                }
            }
        }

        /// <summary>Runs server.</summary>
        public void Run()
        {
            ProcessRequests(_token);
        }

        /// <summary>Stops server's execution.</summary>
        public void Stop()
        {
            // unsubscribe all clients
            foreach (IClient c in _clients.Values)
            {
                c.RequestSent -= ReceivedClientRequestEventHandler;
            }

            // wait until all awaiting requests are processed.
            if (!_awaitingRequests.IsEmpty)
            {
                if (_logginEnabled) _logger.Log("Trying to stop server. Wait until all awaiting requests are processed.");
                while(!_awaitingRequests.IsEmpty)
                {
                    Thread.Sleep(10);
                }
            }
            
            _clients.Clear();
            _tokenSource.Cancel();

            if (_logginEnabled) _logger.Log($"Server has been stopped.");
        }
                
        public void RegisterClient(IClient client)
        {
            if (_clients.ContainsKey(client.ClientId))
            {
                // logging
                if (_logginEnabled) _logger.Log($"Client with ID: {client.ClientId} is already registered on the server");
                
                return;
            }

            if (_clients.TryAdd(client.ClientId, client))
            {
                client.RequestSent += ReceivedClientRequestEventHandler;
                client.SyncData(this.Data, this.Revision);

                // logging                
                if (_logginEnabled) _logger.Log($"Server has registered Client with ID: '{client.ClientId}'");
            }
            else
            {
                // logging
                if (_logginEnabled) _logger.Log($"Server can't registered Client with ID: '{client.ClientId}'");
            }
        }
                
        public void UnregisterClient(IClient client)
        {
            if (!_clients.ContainsKey(client.ClientId))
            {
                // logging
                if (_logginEnabled) _logger.Log($"Server can't unregister Client with ID: {client.ClientId}, because it is not registered");
                
                return;
            }

            if (_clients.TryRemove(client.ClientId, out IClient removedClient))
            {
                removedClient.RequestSent -= ReceivedClientRequestEventHandler;

                // logging
                if (_logginEnabled) _logger.Log($"Server has unregistered Client with ID: '{removedClient.ClientId}'");
            }
            else
            {
                // logging
                if (_logginEnabled) _logger.Log($"Server can't unregister Client with ID: '{client.ClientId}'");
            }
        }

        private void ReceivedClientRequestEventHandler(IRequest request)
        {
            lock (_locker)
            {
                _awaitingRequests.Enqueue(request);
            }            
            
            // logging
            StringBuilder str = new StringBuilder($"Server received operation from Client with ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            str.Append($", operations count: '{request.Operations.Count()}'");
            if (_logginEnabled) _logger.Log(str.ToString());            
        }

        private void ProcessRequests(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // process all incoming request from the queue
                if (_awaitingRequests.IsEmpty || !_awaitingRequests.TryDequeue(out IRequest request)) continue;
                
                lock (_locker)
                {
                    // apply operations over the server data document
                    ApplyOperations(request);

                    // save operations to the server revision log
                    _revisionLog.TryAdd(_revision, request.Operations.ToList());

                    // acknowledge the request
                    IRequest acknowledgedRequest = RequestFactory.CreateRequest(
                        request.ClientId,
                        _revision + 1,
                        request.Operations.ToList(), 
                        true);

                    // broadcast operations to other clients
                    if (BroadcastRequest != null)
                    {
                        BroadcastRequest.Invoke(acknowledgedRequest);
                    }

                    // increment local revision
                    _revision++;
                }
            }
        }

        private void ApplyOperations(IRequest request)
        {
            foreach (IOperation operation in request.Operations)
            {
                switch (operation.OperationType)
                {
                    case OperationType.Insert:
                        OperationProcessor.InsertOperation(_data, operation);
                        break;
                    case OperationType.Delete:
                        OperationProcessor.DeleteOperation(_data, operation);
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException($"Only Insert and Delete operations are supported");
                }
            }
        }
    }
}