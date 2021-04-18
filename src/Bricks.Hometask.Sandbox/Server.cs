using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bricks.Hometask.Sandbox
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
        
        /// <summary>Constructor.</summary>
        public Server()
        {
            _clients = new ConcurrentDictionary<int, IClient>();
            _data = new List<int>();
            _awaitingRequests = new ConcurrentQueue<IRequest>();
            _revisionLog = new ConcurrentDictionary<int, List<IOperation>>();
            _revision = 0;
            _logger = new ConsoleLogger(_color);

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        /// <summary>Runs server.</summary>
        public void Run()
        {
            while (true)
            {
                ProcessRequests(_token);
            }
        }

        /// <summary>Stops server's execution.</summary>
        public void Stop()
        {
            // unsubscribe all clients
            foreach (IClient c in _clients.Values)
            {
                c.OperationSent -= ReceivedClientRequestEventHandler;
            }

            // wait until all awaiting requests are processed.
            if (!_awaitingRequests.IsEmpty)
            {
                _logger.Log("Trying to stop server. Wait until all awaiting requests are processed.");
                while(!_awaitingRequests.IsEmpty)
                {
                    Thread.Sleep(10);
                }
            }
            
            _clients.Clear();
            _tokenSource.Cancel();

            _logger.Log($"Server has been stopped.");
        }
                
        public void RegisterClient(IClient client)
        {
            if (_clients.ContainsKey(client.ClientId))
            {
                // logging
                _logger.Log($"Client with ID: {client.ClientId} is already registered on the server");
                
                return;
            }

            if (_clients.TryAdd(client.ClientId, client))
            {
                client.OperationSent += ReceivedClientRequestEventHandler;
                client.SyncData(this.Data, this.Revision);

                // logging                
                _logger.Log($"Server has registered Client with ID: '{client.ClientId}'");
            }
            else
            {
                // logging
                _logger.Log($"Server can't registered Client with ID: '{client.ClientId}'");
            }
        }
                
        public void UnregisterClient(IClient client)
        {
            if (!_clients.ContainsKey(client.ClientId))
            {
                // logging
                _logger.Log($"Server can't unregister Client with ID: {client.ClientId}, because it is not registered");
                
                return;
            }

            if (_clients.TryRemove(client.ClientId, out IClient removedClient))
            {
                removedClient.OperationSent -= ReceivedClientRequestEventHandler;

                // logging
                _logger.Log($"Server has unregistered Client with ID: '{removedClient.ClientId}'");
            }
            else
            {
                // logging
                _logger.Log($"Server can't unregister Client with ID: '{client.ClientId}'");
            }
        }

        private void ReceivedClientRequestEventHandler(Request request)
        {
            lock (_locker)
            {
                _awaitingRequests.Enqueue(request);
            }            
            
            // logging
            StringBuilder str = new StringBuilder($"Server received operation from Client with ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            str.Append($", operations count: '{request.Operations.Count()}'");
            _logger.Log(str.ToString());            
        }

        private void ProcessRequests(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // process all incoming request from the queue
                if (_awaitingRequests.IsEmpty || !_awaitingRequests.TryDequeue(out IRequest request)) continue;
                
                lock (_locker)
                {
                    // transform request operations according to the current server state
                    IRequest transformedRequest = TransformRequest(request);

                    // apply operations over the server data document
                    ApplyOperations(transformedRequest);

                    // save operations to the server revision log
                    _revisionLog.TryAdd(_revision, transformedRequest.Operations.ToList());

                    // acknowledge the request
                    IRequest acknowledgedRequest = new Request(
                        transformedRequest.ClientId, 
                        _revision + 1,
                        transformedRequest.Operations.ToList(), 
                        true);

                    // broadcast operations to other clients
                    foreach (IClient c in _clients.Values)
                    {
                        c.ReceiveRequestsFromServer(acknowledgedRequest);

                        // logging
                        _logger.Log($"Message is sent by Server to Client with ID: '{c.ClientId}'");
                    }

                    // increment local revision
                    _revision++;
                }
            }

            _logger.Log("Exit ProcessRequests");
        }

        private IRequest TransformRequest(IRequest request)
        {
            if (request.Revision == _revision) return request;
            if (request.Revision > _revision) throw new System.ApplicationException("Request revision is ahead of server revision.");

            IRequest tempRequest = request;
            
            // get operations from revision log since request revision number
            var logOperations = _revisionLog
                .Where(pair => pair.Key > tempRequest.Revision)
                .OrderBy(pair => pair.Key).ToList();

            List<IOperation> transformedOperations = new List<IOperation>();
            transformedOperations.AddRange(tempRequest.Operations);

            foreach (var (revision, operations) in logOperations)
            {
                List<IOperation> temp = OperationTransformer.Transform(transformedOperations, operations).ToList();
                transformedOperations.Clear();
                transformedOperations.AddRange(temp);
                tempRequest = new Request(tempRequest.ClientId, revision, transformedOperations);
            }

            return tempRequest;
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