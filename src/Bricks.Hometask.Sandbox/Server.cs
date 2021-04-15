using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Sandbox
{
    public class Server : IServer
    {
        private readonly object _locker = new object();
        private readonly ReaderWriterLockSlim _slimLocker;
        
        private readonly ConcurrentQueue<IRequest> _awatingRequests;
        private readonly ConcurrentDictionary<int, List<IOperation>> _revisionLog;
        private readonly ConcurrentDictionary<int, IClient> _clients;
        private readonly IList<int> _data;
        private bool _isRunning;
        private int _revision;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _processOperationsCancellationToken;

        /// <summary>Gets current server state.</summary>
        public IEnumerable<int> Data
        {
            get
            {
                _slimLocker.EnterReadLock();
                try
                {
                    foreach(int item in _data)
                    {
                        yield return item;
                    }
                }
                finally
                {
                    _slimLocker.ExitReadLock();
                }
            }
        }

        /// <summary>Gets current server revision number.</summary>
        public int Revision
        {
            get
            {
                _slimLocker.EnterReadLock();
                try
                {
                    return _revision;
                }
                finally
                {
                    _slimLocker.ExitReadLock();
                }
            }
        }
        
        /// <summary>Constructor.</summary>
        public Server()
        {
            _clients = new ConcurrentDictionary<int, IClient>();
            _data = new List<int>();
            _awatingRequests = new ConcurrentQueue<IRequest>();
            _revisionLog = new ConcurrentDictionary<int, List<IOperation>>();
            _revision = 0;
            
            _slimLocker = new ReaderWriterLockSlim();
            _tokenSource = new CancellationTokenSource();
            _processOperationsCancellationToken = new CancellationToken();
        }

        public void InitializeData()
        {
            //TODO: initialize data to start with not empty document
            throw new NotImplementedException();
        }

        /// <summary>Runs server.</summary>
        public void Run()
        {
            _isRunning = true;
            
            // run async job to process awaiting operations
            Task.Run(() => ProcessOperations(_processOperationsCancellationToken));
            
            System.Console.WriteLine($"Server has been started");
            
            while (_isRunning)
            {
                // infinity loop to proceed data. 
                Console.WriteLine("Server is running");
                
                // emulate real world delays
                Thread.Sleep(Timeout.OneSecond);
            }
        }

        /// <summary>Stops server's execution.</summary>
        public void Stop()
        {
            _isRunning = false;

            // unsubscribe all clients
            foreach (var client in _clients.Values)
            {
                client.OperationSent -= ReceiveClientRequestEventHandler;
            }
            _clients.Clear();
            
            System.Console.WriteLine($"Server has been stopped");
        }
        
        /// <summary>Registers a new client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void RegisterClient(IClient client)
        {
            // do not allow to register new clients if server is not running
            if (!_isRunning) return;

            if (_clients.ContainsKey(client.ClientId))
            {
                // logging
                Console.WriteLine($"Client with ID: {client.ClientId} is already registered on the server");
                
                return;
            }

            if (_clients.TryAdd(client.ClientId, client))
            {
                client.OperationSent += ReceiveClientRequestEventHandler;
            
                // logging
                Console.WriteLine($"Server has registered Client with ID: '{client.ClientId}'");                
            }
            else
            {
                // logging
                Console.WriteLine($"Server can't registered Client with ID: '{client.ClientId}'");
            }
        }

        /// <summary>Unregisters a client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void UnregisterClient(IClient client)
        {
            if (!_clients.ContainsKey(client.ClientId))
            {
                // logging
                Console.WriteLine($"Server can't unregister Client with ID: {client.ClientId}, because it is not registered");
                
                return;
            }

            if (_clients.TryRemove(client.ClientId, out IClient removedClient))
            {
                removedClient.OperationSent -= ReceiveClientRequestEventHandler;

                // logging
                Console.WriteLine($"Server has unregistered Client with ID: '{removedClient.ClientId}'");
            }
            else
            {
                // logging
                Console.WriteLine($"Server can't unregister Client with ID: '{client.ClientId}'");
            }
        }

        private void ReceiveClientRequestEventHandler(Request request)
        {
            _awatingRequests.Enqueue(request);
            
            // logging
            StringBuilder str = new StringBuilder($"Server received operation from Client ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            str.Append($", operations count: '{request.Operations.Count()}'");
            Console.WriteLine(str.ToString());
        }

        private void ProcessOperations(CancellationToken token)
        {
            // infinite loop to process incoming client requests with operations 
            while (_isRunning)
            {
                // process all incoming request from the queue
                while (!_awatingRequests.IsEmpty)
                {
                    IRequest request;

                    if (_awatingRequests.TryDequeue(out request))
                    {
                        if (request.Revision < _revision)
                        {
                            int requestRevision = request.Revision;
                            var logOperations = _revisionLog
                                .Where(pair => pair.Key >= requestRevision)
                                .OrderBy(pair => pair.Key);

                            foreach (var (revision, operations) in logOperations)
                            {
                                var transformedOperations = OperationTransformer.Transform(request.Operations, operations);
                                request = new Request(request.ClientId, revision, transformedOperations);
                            }
                        }
                        
                        // apply operations over the server data document
                        foreach (var operation in request.Operations)
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
                                    throw new ArgumentOutOfRangeException($"Only Insert and Delete operations are supported");
                            }    
                        }
                        
                        // save operations to the server revision log
                        _revisionLog.TryAdd(_revision, request.Operations.ToList());
                        
                        // send acknowledgment to client
                        _clients.Values
                            .FirstOrDefault(i => i.ClientId == request.ClientId)?
                            .ReceiveOperationsFromServer(new Request(request.ClientId, _revision + 1, request.Operations, true));
                        
                        // broadcast operations to other clients
                        foreach (var client in _clients.Values)
                        {
                            if (client.ClientId == request.ClientId) continue;
                            client.ReceiveOperationsFromServer(new Request( request.ClientId,_revision + 1, request.Operations.ToList()));
                        }
                        
                        // increment local revision
                        Interlocked.Increment(ref _revision);
                    }
                }
            }

            // clear request queue if server is shut down
            if (token.IsCancellationRequested)
            {
                _awatingRequests.Clear();
            }
        }
    }
}