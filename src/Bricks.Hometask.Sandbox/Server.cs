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
        private readonly ReaderWriterLockSlim _slimLocker = new ReaderWriterLockSlim();
        private readonly ManualResetEvent _event = new ManualResetEvent(false);

        private readonly System.ConsoleColor _color = System.ConsoleColor.Red;
        private readonly ConsoleLogger _logger;

        private readonly ConcurrentQueue<IRequest> _awatingRequests;
        private readonly ConcurrentDictionary<int, List<IOperation>> _revisionLog;
        private readonly ConcurrentDictionary<int, IClient> _clients;
        private readonly IList<int> _data;
        private bool _isRunning;
        private int _revision;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
                
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
            get
            {
                _slimLocker.EnterReadLock();
                try
                {
                    return _isRunning;
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
            _logger = new ConsoleLogger(_color);

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        /// <summary>Runs server.</summary>
        public void Run()
        {
            //_isRunning = true;
            
            // run async job to process awaiting operations
            Task.Run(() => ProcessRequests(_token));
            
            //System.Console.WriteLine($"Server has been started.");
            
            while (true)
            {
                // infinity loop to proceed data. 
                //System.Console.WriteLine("Server is running");
                
                // emulate real world delays
                //Thread.Sleep(Timeout.OneSecond);
            }
        }

        /// <summary>Stops server's execution.</summary>
        public void Stop()
        {
            //if(!_isRunning) System.Console.WriteLine("Server is already stopped");

            // unsubscribe all clients
            foreach (IClient c in _clients.Values)
            {
                c.OperationSent -= ReceivedClientRequestEventHandler;
            }

            // wait untill all awaiting requests are processed.
            if (!_awatingRequests.IsEmpty)
            {
                _logger.Log("Trying to stop server. Wait untill all awaiting requests are processed.");
                while(!_awatingRequests.IsEmpty)
                {
                    Thread.Sleep(10);
                }
            }
            
            _clients.Clear();
            _tokenSource.Cancel();
            _isRunning = false;

            _logger.Log($"Server has been stopped.");
        }
                
        public void RegisterClient(IClient client)
        {
            // do not allow to register new clients if server is not running
            //if (!_isRunning) return;

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
            //_event.WaitOne();

            _awatingRequests.Enqueue(request);            
            
            // logging
            StringBuilder str = new StringBuilder($"Server received operation from Client with ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            str.Append($", operations count: '{request.Operations.Count()}'");
            _logger.Log(str.ToString());            
        }

        private void ProcessRequests(CancellationToken token)
        {
            _isRunning = true;

            // infinite loop to process incoming client requests with operations 
            while (!token.IsCancellationRequested)
            {
                // process all incoming request from the queue
                //while (!_awatingRequests.IsEmpty)
                //{
                //_event.Reset();
                if (!_awatingRequests.IsEmpty && _awatingRequests.TryDequeue(out IRequest request))
                {
                    // transform request operations according to the current server state
                    IRequest transformedRequest = TransformRequest(request);

                    // apply operations over the server data document
                    ApplyOperations(transformedRequest);
                        
                    // save operations to the server revision log
                    _revisionLog.TryAdd(_revision, transformedRequest.Operations.ToList());

                    // acknowledge the request
                    IRequest acknowledgedRequest = new Request(transformedRequest.ClientId, _revision + 1, transformedRequest.Operations.ToList(), true);
                        
                    // broadcast operations to other clients
                    foreach (IClient c in _clients.Values)
                    {                            
                        c.ReceiveRequestsFromServer(acknowledgedRequest);

                        // logging
                        _logger.Log($"Message is sent by Server to Client with ID: '{c.ClientId}'");
                    }
                        
                    // increment local revision
                    Interlocked.Increment(ref _revision);
                }
                //_event.Set();

                Thread.Sleep(System.TimeSpan.FromMilliseconds(10));
                //}
            }

            _logger.Log("Exit ProcessRequests");

            // clear request queue if server is shut down
            //if (token.IsCancellationRequested)
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
                transformedOperations.AddRange(OperationTransformer.Transform(temp, operations));
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