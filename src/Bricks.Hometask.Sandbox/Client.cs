using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Sandbox
{
    public class Client : IClient
    {
        private readonly ReaderWriterLockSlim _slimLocker;
        
        private IList<int> _data;
        private int _revision;
        private bool _isRunning;
        private readonly ConcurrentQueue<IRequest> _awaitingOperations;
        private readonly ConcurrentQueue<IRequest> _receivedOperations;
        private readonly ConcurrentQueue<IOperation> _operationsBuffer;
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _sendToServerCancellationToken;
        
        public int ClientId { get; }
        
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

        public bool CanBeStopped
        {
            get { return _operationsBuffer.IsEmpty && _awaitingOperations.IsEmpty && _receivedOperations.IsEmpty; }
        }
        
        public event OperationSentEventHandler OperationSent;
        
        /// <summary>Constructor.</summary>
        /// <param name="clientId">Unique client identifier.</param>
        public Client(int clientId)
        {
            ClientId = clientId;
            _data = new List<int>();
            _awaitingOperations = new ConcurrentQueue<IRequest>();
            _operationsBuffer = new ConcurrentQueue<IOperation>();
            _receivedOperations = new ConcurrentQueue<IRequest>();
            _slimLocker = new ReaderWriterLockSlim();

            _tokenSource = new CancellationTokenSource();
            _sendToServerCancellationToken = new CancellationToken();
        }
        
        public void SyncData(IEnumerable<int> data, int revision)
        {
            _slimLocker.EnterWriteLock();
            try
            {
                _data = new List<int>(data.ToList());
                _revision = revision;
            }
            finally
            {
                _slimLocker.ExitWriteLock();
            }
        }
        
        public void Run()
        {
            _isRunning = true;
            
            // run async job to send operations to server
            Task.Run(() => SendOperationsToServer(_sendToServerCancellationToken));
            
            // logging
            System.Console.WriteLine($"Client with ID: '{ClientId}' has been started");
            
            while (_isRunning)
            {
                // infinity loop to proceed data
               System.Console.WriteLine($"Client with ID: '{ClientId}' is running");

               // emulate real world operation
               Thread.Sleep(Timeout.OneSecond);
            }
        }
        
        public void Stop()
        {
            if (!_isRunning) System.Console.WriteLine("Client is already stopped");

            _isRunning = false;
            _tokenSource.Cancel();
            
            // logging
            System.Console.WriteLine($"Client with ID: '{ClientId}' has been stopped");
        }

        public void PushOperation(IOperation operation)
        {
            if (!_isRunning) System.Console.WriteLine($"Client with ID: {ClientId} can't process operation, because client is not running");

            ProcessOperation(operation);
            
            // logging
            System.Console.WriteLine($"Operation occured at Client ID: '{ClientId}', type: '{operation.OperationType}'");
        }
        
        public void ReceiveOperationsFromServer(IRequest request)
        {
            if (!_isRunning) System.Console.WriteLine($"Client with ID: {ClientId} can't sync with the server, because client is not running");

            _receivedOperations.Enqueue(request);
        }
        
        private void SendOperationsToServer(CancellationToken token)
        {
            while (_isRunning)
            {
                while (!_receivedOperations.IsEmpty)
                {
                    if (!_receivedOperations.TryDequeue(out IRequest r)) continue;
                    
                    // check if request is acknowledgment for the awaiting operation 
                    if (r.IsAcknowledged && 
                        r.ClientId == ClientId &&
                        _awaitingOperations.Count() != 0 &&
                        r.Operations.All(o1 => _awaitingOperations.First().Operations.Any(o2 => o1.Timestamp == o2.Timestamp)))
                    {
                        _awaitingOperations.Clear();
                        _revision = r.Revision;

                        // logging
                        System.Console.WriteLine($"Client with ID: '{ClientId}' recieved ack message");
                        continue;
                    }
                    
                    // transform operations in buffer over the received messages
                    List<IOperation> operations = OperationTransformer.Transform(_operationsBuffer.ToList(), r.Operations).ToList();
                    _operationsBuffer.Clear();
                    foreach (IOperation operation in operations)
                    {
                        _operationsBuffer.Enqueue(operation);
                    }
                    
                    ApplyOperations(r.Operations.ToList());
                }

                // awaiting acknowledgement from server
                if (!_awaitingOperations.IsEmpty)
                {
                    //TODO: increase timout on each iteration
                    continue;
                }

                // do nothing if buffer is empty
                if (_operationsBuffer.IsEmpty) continue;

                // if no subscriber (server) attached do not send operations
                if (OperationSent == null) continue;
                
                // send new bunch of operations from buffer if no awaiting operations
                Request request = new Request(ClientId, _revision, _operationsBuffer.ToList());
                OperationSent.Invoke(request);

                _awaitingOperations.Enqueue(request);
                _operationsBuffer.Clear();

                //TODO: add timeout
            }

            if (!token.IsCancellationRequested) return;
            
            // clear queues if client is shut down
            _awaitingOperations.Clear();
            _operationsBuffer.Clear();
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
                    _operationsBuffer.Enqueue(new Operation(OperationType.Delete, operation.Index));
                    _operationsBuffer.Enqueue(new Operation(OperationType.Insert, operation.Index, operation.Value));
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