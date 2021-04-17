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
        private readonly ReaderWriterLockSlim _slimLocker;
        
        private readonly ConcurrentQueue<IRequest> _awatingRequests;
        private readonly ConcurrentDictionary<int, List<IOperation>> _revisionLog;
        private readonly ConcurrentDictionary<int, IClient> _clients;
        private readonly IList<int> _data;
        private bool _isRunning;
        private int _revision;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _processRequestsCancellationToken;
                
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

        public bool IsAlive
        {
            get { return _isRunning; }
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
            _processRequestsCancellationToken = new CancellationToken();
        }

        public void InitializeData()
        {
            //TODO: initialize data to start with not empty document
            throw new System.NotImplementedException();
        }

        /// <summary>Runs server.</summary>
        public void Run()
        {
            _isRunning = true;
            
            // run async job to process awaiting operations
            Task.Run(() => ProcessRequests(_processRequestsCancellationToken));
            
            System.Console.WriteLine($"Server has been started");
            
            while (_isRunning)
            {
                // infinity loop to proceed data. 
                System.Console.WriteLine("Server is running");
                
                // emulate real world delays
                Thread.Sleep(Timeout.OneSecond);
            }
        }

        /// <summary>Stops server's execution.</summary>
        public void Stop()
        {
            if(!_isRunning) System.Console.WriteLine("Server is already stopped");

            if (!_awatingRequests.IsEmpty)
            {
                System.Console.WriteLine("Trying to stop server");
                while(!_awatingRequests.IsEmpty)
                {
                    System.Console.Write(".");
                    Thread.Sleep(10);
                }
            }

            _isRunning = false;

            // unsubscribe all clients
            foreach (IClient c in _clients.Values)
            {
                c.OperationSent -= ReceivedClientRequestEventHandler;
            }
            _clients.Clear();
            
            System.Console.WriteLine($"Server has been stopped");
        }
                
        public void RegisterClient(IClient client)
        {
            // do not allow to register new clients if server is not running
            if (!_isRunning) return;

            if (_clients.ContainsKey(client.ClientId))
            {
                // logging
                System.Console.WriteLine($"Client with ID: {client.ClientId} is already registered on the server");
                
                return;
            }

            if (_clients.TryAdd(client.ClientId, client))
            {
                client.OperationSent += ReceivedClientRequestEventHandler;

                // logging
                System.Console.WriteLine($"Server has registered Client with ID: '{client.ClientId}'");                
            }
            else
            {
                // logging
                System.Console.WriteLine($"Server can't registered Client with ID: '{client.ClientId}'");
            }
        }
                
        public void UnregisterClient(IClient client)
        {
            if (!_clients.ContainsKey(client.ClientId))
            {
                // logging
                System.Console.WriteLine($"Server can't unregister Client with ID: {client.ClientId}, because it is not registered");
                
                return;
            }

            if (_clients.TryRemove(client.ClientId, out IClient removedClient))
            {
                removedClient.OperationSent -= ReceivedClientRequestEventHandler;

                // logging
                System.Console.WriteLine($"Server has unregistered Client with ID: '{removedClient.ClientId}'");
            }
            else
            {
                // logging
                System.Console.WriteLine($"Server can't unregister Client with ID: '{client.ClientId}'");
            }
        }

        private void ReceivedClientRequestEventHandler(Request request)
        {
            _awatingRequests.Enqueue(request);
            
            // logging
            StringBuilder str = new StringBuilder($"Server received operation from Client ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            str.Append($", operations count: '{request.Operations.Count()}'");
            System.Console.WriteLine(str.ToString());
        }

        private void ProcessRequests(CancellationToken token)
        {
            // infinite loop to process incoming client requests with operations 
            while (_isRunning)
            {
                // process all incoming request from the queue
                while (!_awatingRequests.IsEmpty)
                {
                    if (_awatingRequests.TryDequeue(out IRequest request))
                    {
                        // transform request operations according to the current server state
                        request = TransformRequest(request);

                        // apply operations over the server data document
                        ApplyOperations(request);
                        
                        // save operations to the server revision log
                        _revisionLog.TryAdd(_revision, request.Operations.ToList());

                        // acknowledge the request
                        IRequest acknowledgedRequest = new Request(request.ClientId, _revision + 1, request.Operations.ToList(), true);
                        
                        // broadcast operations to other clients
                        foreach (IClient c in _clients.Values)
                        {                            
                            c.ReceiveOperationsFromServer(acknowledgedRequest);
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

        private IRequest TransformRequest(IRequest request)
        {
            if (request.Revision == _revision) return request;
            if (request.Revision > _revision) throw new System.ApplicationException("Request revision is ahead of server revision.");

            IRequest tempRequest = request;
            
            // get operations from revision log since request revision number
            var logOperations = _revisionLog
                .Where(pair => pair.Key >= tempRequest.Revision)
                .OrderBy(pair => pair.Key).ToList();

            List<IOperation> transformedOperations = new List<IOperation>();
            transformedOperations.AddRange(tempRequest.Operations);

            foreach (var (revision, operations) in logOperations)
            {
                List<IOperation> temp = OperationTransformer.Transform(transformedOperations, operations).ToList();
                transformedOperations.Clear();
                transformedOperations.AddRange(OperationTransformer.Transform(transformedOperations, operations));
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