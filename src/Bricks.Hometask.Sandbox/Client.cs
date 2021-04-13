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
        private bool _isRunning;
        private ConcurrentQueue<Request> _awatingOperations;
        private ConcurrentQueue<IOperation> _operationsBuffer;
        private ConcurrentDictionary<int, IEnumerable<IOperation>> _receivedOperations;
        private int _revision;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _sendToServerCancellationToken;
        
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
        
        public event OperationSentEventHandler OperationSent;
        
        /// <summary>Constructor.</summary>
        /// <param name="clientId">Unique client identifier.</param>
        public Client(int clientId)
        {
            ClientId = clientId;
            _data = new List<int>();
            _awatingOperations = new ConcurrentQueue<Request>();
            _operationsBuffer = new ConcurrentQueue<IOperation>();
            _receivedOperations = new ConcurrentDictionary<int, IEnumerable<IOperation>>();
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
                // infinity loop to proceed data. 
               System.Console.WriteLine($"Client with ID: '{ClientId}' is running");

               // emulate real world operation
               Thread.Sleep(Timeout.OneSecond);
            }
        }
        
        public void Stop()
        {
            _isRunning = false;
            _tokenSource.Cancel();
            
            // logging
            System.Console.WriteLine($"Client with ID: '{ClientId}' has been stopped");
        }

        public void PushOperation(IOperation operation)
        {
            ProcessOperation(operation);
            
            // logging
            System.Console.WriteLine($"Operation received at Client ID: '{ClientId}', type: '{operation.OperationType}'");
        }
        
        public void AcknowledgeOperation(int revision)
        {
            _slimLocker.EnterWriteLock();
            try
            {
                _revision = revision;
                _awatingOperations.Clear();
            }
            finally
            {
                _slimLocker.ExitWriteLock();
            }
        }
        
        public void ReceiveOperationsFromServer(int revision, IEnumerable<IOperation> operations)
        {
            _receivedOperations.TryAdd(revision, operations.ToList());
                
            //TODO: transform operations in awaiting operations over the received messages
            //TODO: transform operations in buffer over the received messages
            //TODO: lock is required for transformation and data update

            ApplyOperations(operations.ToList());
        }
        
        private void SendOperationsToServer(CancellationToken token)
        {
            while (_isRunning)
            {
                _slimLocker.EnterUpgradeableReadLock();
                try
                {
                    // awaiting acknowledgement from server
                    if (_awatingOperations.Count != 0)
                    {
                        //TODO: increase timout on each iteration
                        continue;
                    }

                    // do nothing if buffer is empty
                    if (_operationsBuffer.Count == 0)
                    {
                        continue;
                    }

                    // if no subscriber (server) attached do not send operations
                    if (OperationSent != null)
                    {
                        // send new bunch of operations from buffer if no awaiting operations
                        Request request = new Request(ClientId, _revision, _operationsBuffer.ToList());
                        OperationSent.Invoke(request);
                        
                        _slimLocker.EnterWriteLock();
                        try
                        {
                            _awatingOperations.Enqueue(request);
                            _operationsBuffer.Clear();
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

                //TODO: add timeout
            }

            // clear queues if client is shut down
            if (token.IsCancellationRequested)
            {
                _awatingOperations.Clear();
                _operationsBuffer.Clear();
            }
        }

        private void ApplyOperations(IEnumerable<IOperation> operations)
        {
            foreach (var operation in operations)
            {
                switch (operation.OperationType)
                {
                    case OperationType.Insert:
                        if (operation.Index > -1 && operation.Index <= _data.Count)
                        {
                            _slimLocker.EnterWriteLock();
                            try
                            {
                                _data.Insert(operation.Index, operation.Value.Value);
                            }
                            finally
                            {
                                _slimLocker.ExitWriteLock();
                            }
                        }
                        break;
                    case OperationType.Delete:
                        if (_data.Count > 0 && operation.Index > -1 && operation.Index < _data.Count)
                        {
                            _slimLocker.EnterWriteLock();
                            try
                            {
                                _data.RemoveAt(operation.Index);
                            }
                            finally
                            {
                                _slimLocker.ExitWriteLock();
                            }
                        }
                        break;
                }
            }
        }
        
        private void ProcessOperation(IOperation operation)
        {
            _slimLocker.EnterUpgradeableReadLock();
            try
            {
                switch (operation.OperationType)
                {
                    case OperationType.Insert:
                        if (operation.Index > -1 && operation.Index <= _data.Count)
                        {
                            _slimLocker.EnterWriteLock();
                            try
                            {
                                InsertOperation(operation);
                            }
                            finally
                            {
                                _slimLocker.ExitWriteLock();
                            }
                        }
                        break;
                    case OperationType.Update:
                        if (_data.Count > 0 && operation.Index > -1 && operation.Index < _data.Count)
                        {
                            _slimLocker.EnterWriteLock();
                            try
                            {
                                UpdateOperation(operation);
                            }
                            finally
                            {
                                _slimLocker.ExitWriteLock();
                            }
                        }
                        break;
                    case OperationType.Delete:
                        if (_data.Count > 0 && operation.Index > -1 && operation.Index < _data.Count)
                        {
                            _slimLocker.EnterWriteLock();
                            try
                            {
                                DeleteOperation(operation);
                            }
                            finally
                            {
                                _slimLocker.ExitWriteLock();
                            }
                        }
                        break;
                }
            }
            finally
            {
                _slimLocker.ExitUpgradeableReadLock();
            }
        }

        private void InsertOperation(IOperation operation)
        {
            _data.Insert(operation.Index, operation.Value.Value);
            _operationsBuffer.Enqueue(operation);
        }

        private void UpdateOperation(IOperation operation)
        {
            _data.RemoveAt(operation.Index);
            _operationsBuffer.Enqueue(new Operation(OperationType.Delete, operation.Index));
            _data.Insert(operation.Index, operation.Value.Value);
            _operationsBuffer.Enqueue(new Operation(OperationType.Insert, operation.Index, operation.Value));
        }
        
        private void DeleteOperation(IOperation operation)
        {
            _data.RemoveAt(operation.Index);
            _operationsBuffer.Enqueue(operation);
        }
    }
}