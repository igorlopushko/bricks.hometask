using System;
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
        private readonly ReaderWriterLockSlim _slimLocker;
        
        private ConcurrentQueue<IRequest> _awatingRequests;
        private Dictionary<int, List<IOperation>> _revisionLog;
        private IList<int> _data;
        private IList<IClient> _clients;
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
            _clients = new List<IClient>();
            _data = new List<int>();
            _awatingRequests = new ConcurrentQueue<IRequest>();
            _revisionLog = new Dictionary<int, List<IOperation>>();
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
            //TODO: UNCOMMENT!!!
            //Task.Run(() => ProcessOperations(_processOperationsCancellationToken));
            
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
            foreach (var client in _clients)
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

            _slimLocker.EnterUpgradeableReadLock();
            try
            {
                client.SyncData(_data.ToList(), _revision);
                if (_clients.All(c => c.ClientId != client.ClientId))
                {
                    _slimLocker.EnterWriteLock();
                    try
                    {
                        _clients.Add(client);
                    }
                    finally
                    {
                        _slimLocker.ExitWriteLock();
                    }
                }
            }
            finally 
            {
                _slimLocker.ExitUpgradeableReadLock();
            }
            
            client.OperationSent += ReceiveClientRequestEventHandler;
            
            // logging
            Console.WriteLine($"Register client with ID: '{client.ClientId}'");
        }

        /// <summary>Unregisters a client within the server.</summary>
        /// <param name="client">Client object instance.</param>
        public void UnregisterClient(IClient client)
        {
            _clients.RemoveAt(_clients.IndexOf(client));
            client.OperationSent -= ReceiveClientRequestEventHandler;
            
            // logging
            Console.WriteLine($"Unregister client with ID: '{client.ClientId}'");
        }

        private void ReceiveClientRequestEventHandler(Request request)
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

        private void ProcessOperations(CancellationToken token)
        {
            while (_isRunning)
            {
                while (!_awatingRequests.IsEmpty)
                {
                    IRequest request;

                    if (_awatingRequests.TryDequeue(out request))
                    {
                        if (request.Revision != _revision + 1)
                        {
                            //TODO: transform operations over the revision log
                            //TODO: starting with this request revision and till the end of the revision log
                        }
                        
                        //TODO: apply operations over the data document
                        foreach (var operation in request.Operations)
                        {
                            switch (operation.OperationType)
                            {
                                case OperationType.Insert:
                                    _data.Insert(operation.Index, operation.Value.Value);
                                    break;
                                case OperationType.Delete:
                                    _data.RemoveAt(operation.Index);
                                    break;
                            }    
                        }
                        
                        //TODO: save operations to revision log
                        _revisionLog.Add(_revision, request.Operations.ToList());
                        
                        //TODO: send acknowledgment to client
                        _revision++;
                        _clients.FirstOrDefault(i => i.ClientId == request.ClientId)?.AcknowledgeOperation(_revision);
                        
                        //TODO: broadcast operations to other clients
                        foreach (var client in _clients)
                        {
                            if (client.ClientId == request.ClientId) continue;
                            client.ReceiveOperationsFromServer(_revision, request.Operations.ToList());
                        }
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