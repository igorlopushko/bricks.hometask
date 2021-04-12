using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Sandbox
{
    public class Client : IClient
    {
        private readonly object _lockObj = new object();
        
        private IList<int> _data;
        private bool _isRunning;
        private Queue<Request> _awatingOperations;
        private Queue<IOperation> _operationsBuffer;
        private int _revision;
        CancellationTokenSource _tokenSource;
        CancellationToken _sendToServerCancellationToken;
        
        /// <summary>Gets client unique identifier</summary>
        public int ClientId { get; }

        /// <summary>Gets current client state</summary>
        public IEnumerable<int> Data
        {
            get
            {
                foreach(int item in _data)
                {
                    yield return item;
                }
            }
        }

        /// <summary>An event that occurs when operation is sent to the server</summary>
        public event OperationSentEventHandler OperationSent;
        
        public Client(int clientId)
        {
            ClientId = clientId;
            _data = new List<int>();
            _awatingOperations = new Queue<Request>();
            _operationsBuffer = new Queue<IOperation>();

            _tokenSource = new CancellationTokenSource();
            _sendToServerCancellationToken = new CancellationToken();
        }

        public void InitData(IEnumerable<int> data, int revision)
        {
            lock (_lockObj)
            {
                _data = new List<int>(data.ToList());
                _revision = revision;
            }
        }

        public void Run()
        {
            _isRunning = true;
            
            // run send to server job
            Task.Run(() => SendOperationToServer(_sendToServerCancellationToken));
            
            while (_isRunning)
            {
                // infinity loop to proceed data. 
               System.Console.WriteLine($"Client with ID: '{ClientId}' is running");

               lock (_lockObj)
               {
                   IOperation operation = GenerateRandomOperation();
                   ProcessOperation(operation);
               }
               
               // emulate real world operation
               Thread.Sleep(Timeout.OneSecond);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            
            _tokenSource.Cancel();
        }

        private void SendOperationToServer(CancellationToken token)
        {
            while (_isRunning)
            {
                lock (_lockObj)
                {
                    if (_awatingOperations.Count != 0)
                    {
                        //TODO: increase timout on each iteration
                        continue;
                    }

                    if (_operationsBuffer.Count == 0)
                    {
                        continue;
                    }

                    Request request = new Request(ClientId, _revision, _operationsBuffer.ToList());
                    if (OperationSent != null)
                    {
                        OperationSent.Invoke(request);
                        _awatingOperations.Enqueue(request);
                        _operationsBuffer.Clear();
                    }
                }
            }

            // clear queues if client is shut down
            if (token.IsCancellationRequested)
            {
                _awatingOperations.Clear();
                _operationsBuffer.Clear();
            }
        }
        
        private void ProcessOperation(IOperation operation)
        {
            switch (operation.OperationType)
            {
                case OperationType.Insert:
                    if (operation.Index > -1 && operation.Index <= _data.Count)
                    {
                        _data.Insert(operation.Index, operation.Value.Value);
                        _operationsBuffer.Enqueue(operation);
                        //OperationOccured?.Invoke(operation);
                    }
                    break;
                case OperationType.Delete:
                    if (_data.Count > 0 && operation.Index > -1 && operation.Index < _data.Count)
                    {
                        _data.RemoveAt(operation.Index);
                        _operationsBuffer.Enqueue(operation);
                        //OperationOccured?.Invoke(operation);
                    }
                    break;
            }
        }
        
        private IOperation GenerateRandomOperation()
        {
            OperationType type = RandomGenerator.GetOperation();
            int index = RandomGenerator.GetIndex(_data.Count);

            switch (type)
            {
                case OperationType.Insert:
                    return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
                case OperationType.Delete:
                    return new Operation(OperationType.Delete, index);
                default:
                    return new Operation(OperationType.Insert, index, RandomGenerator.GetNumber(int.MinValue, int.MaxValue));
            }
        }
    }
}