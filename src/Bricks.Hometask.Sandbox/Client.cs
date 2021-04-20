using Bricks.Hometask.Base;
using Bricks.Hometask.Utility;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.OperationTransformation.Console
{
    public class Client : IClient
    {
        private readonly object _locker = new object();

        private readonly System.ConsoleColor _color = System.ConsoleColor.Blue;
        private readonly ConsoleLogger _logger;

        private IServer _server;

        private IList<int> _data;
        private int _revision;
        private readonly ConcurrentQueue<IRequest> _awaitingRequests;
        private readonly ConcurrentQueue<IRequest> _receivedRequests;
        private readonly ConcurrentQueue<IOperation> _operationsBuffer;
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;
        private readonly bool _logginEnabled;

        public int ClientId { get; }
        
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

        public event RequestSentEventHandler RequestSent;

        /// <summary>Constructor.</summary>
        /// <param name="clientId">Unique client identifier.</param>
        public Client(IServer server, int clientId, IConfigurationRoot configuration)
        {
            ClientId = clientId;
            _data = new List<int>();
            _awaitingRequests = new ConcurrentQueue<IRequest>();
            _operationsBuffer = new ConcurrentQueue<IOperation>();
            _receivedRequests = new ConcurrentQueue<IRequest>();
            _logger = new ConsoleLogger(_color);

            _server = server;
            _server.BroadcastRequest += Server_BroadcastRequest;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            _logginEnabled = bool.Parse(configuration["LoggingEnabled"]);
        }

        private void Server_BroadcastRequest(IRequest request)
        {
            _receivedRequests.Enqueue(request);
            if (_logginEnabled) _logger.Log($"Client with ID: '{ClientId}' received new request from the server");
        }

        public void SyncData(IEnumerable<int> data, int revision)
        {
            // skip data sync if data is in initial state
            if (data.Count() == 0 && revision == 0) return;

            lock(_locker)
            {
                _data = new List<int>(data.ToList());
                _revision = revision;
            }
        }
        
        public void Run()
        {   
            // run async job to send requests to server
            Task.Run(() => SendRequests(_token));

            // logging
            if (_logginEnabled) _logger.Log($"Client with ID: '{ClientId}' has been started");
            
            while (true)
            {
                HandleReceivedRequests();
            }
        }
        
        public void Stop()
        {
            if (_awaitingRequests.Count() != 0 || _operationsBuffer.Count() != 0)
            {
                if (_logginEnabled) _logger.Log($"Trying to stop Client with ID: '{ClientId}'");
                while (_awaitingRequests.Count() != 0 || _operationsBuffer.Count() != 0)
                {
                    Thread.Sleep(100);
                }
            }

            _tokenSource.Cancel();

            _server.BroadcastRequest -= Server_BroadcastRequest;

            // logging
            if (_logginEnabled) _logger.Log($"Client with ID: '{ClientId}' has been stopped");
        }

        public void PushOperation(IOperation operation)
        {
            lock (_locker)
            {
                ProcessOperation(operation);
            }

            // logging
            if (_logginEnabled) _logger.Log($"Operation occured at Client with ID: '{ClientId}', type: '{operation.OperationType}'");
        }

        private void HandleReceivedRequests()
        {
            if (_receivedRequests.IsEmpty) return;
            if (!_receivedRequests.TryDequeue(out IRequest r)) return;

            lock (_locker)
            {
                // check if request is acknowledgment for the awaiting request 
                if (r.IsAcknowledged && r.ClientId == ClientId && _awaitingRequests.Count() != 0 &&
                    r.Operations.All(o1 => _awaitingRequests.First().Operations.Any(o2 => o1.Timestamp == o2.Timestamp)))
                {
                    _awaitingRequests.Clear();
                    _revision = r.Revision;

                    // logging
                    if (_logginEnabled) _logger.Log($"Client with ID: '{ClientId}' recieved ack message");
                    return;
                }                

                // transform operations in buffer over the received messages
                if (!_operationsBuffer.IsEmpty)
                {
                    List<IOperation> transformedOperations = OperationTransformer.Transform(_operationsBuffer.ToList(), r.Operations).ToList();
                    _operationsBuffer.Clear();
                    foreach (IOperation operation in transformedOperations)
                    {
                        _operationsBuffer.Enqueue(operation);
                    }                    
                }
                _revision = r.Revision;

                ApplyOperations(r.Operations.ToList());

                if (_logginEnabled) _logger.Log($"Client with ID: '{ClientId}' processed incoming request");
            }
        }

        private void SendRequests(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                lock (_locker)
                {
                    // awaiting acknowledgement from server
                    if (!_awaitingRequests.IsEmpty) continue;

                    // do nothing if buffer is empty
                    if (_operationsBuffer.IsEmpty) continue;

                    // if no subscriber (server) attached do not send operations
                    if (RequestSent == null) continue;

                    // send new bunch of operations from buffer if no awaiting operations
                    IRequest request = RequestFactory.CreateRequest(ClientId, _revision, _operationsBuffer.ToList());
                    RequestSent.Invoke(request);

                    _awaitingRequests.Enqueue(request);
                    _operationsBuffer.Clear();

                    if (_logginEnabled) _logger.Log($"Client with ID: '{ClientId}' sent new request to the server");
                }
            }
        }

        private void ApplyOperations(IEnumerable<IOperation> operations)
        {
            foreach (IOperation operation in operations)
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
        
        private void ProcessOperation(IOperation operation)
        {
            switch (operation.OperationType)
            {
                case OperationType.Insert:
                    OperationProcessor.InsertOperation(_data, operation);
                    _operationsBuffer.Enqueue(operation);
                    break;
                case OperationType.Update:
                    OperationProcessor.UpdateOperation(_data, operation);
                    _operationsBuffer.Enqueue(OperationFactory.CreateOperation(OperationType.Delete, operation.Index, operation.ClientId, default, timestamp: operation.Timestamp));
                    _operationsBuffer.Enqueue(OperationFactory.CreateOperation(OperationType.Insert, operation.Index, operation.ClientId, operation.Value.Value, timestamp: operation.Timestamp));
                    break;
                case OperationType.Delete:
                    OperationProcessor.DeleteOperation(_data, operation);
                    _operationsBuffer.Enqueue(operation);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException($"Operation {operation.OperationType} is not supported");
            }
        }
    }
}