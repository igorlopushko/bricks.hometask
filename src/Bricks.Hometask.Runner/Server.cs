using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Runner

{
    public class Server : IDisposable
    {
        private readonly object _locker = new object(); 
        private readonly ReaderWriterLockSlim _slimLocker = new ReaderWriterLockSlim();
        
        private readonly System.ConsoleColor _color = System.ConsoleColor.Red;
        private readonly ConsoleLogger _logger;
        
        private readonly TcpListener _listener;
        private readonly int _port;

        private readonly ConcurrentDictionary<int, TcpClient> _clients;
        private readonly IList<int> _data;
        private readonly ConcurrentDictionary<int, List<IOperation>> _revisionLog;
        private int _revision;
        private bool disposed;
        
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
        
        public Server(int port) 
        {
            _clients = new ConcurrentDictionary<int, TcpClient>();
            _data = new List<int>();
            _revisionLog = new ConcurrentDictionary<int, List<IOperation>>();
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
                        
            while (true)
            {                
                TcpClient client = _listener.AcceptTcpClient();
                Task.Run(() => HandleClientRequest(client));
            }
        }
        
        ~Server()
        {
            Dispose (false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // release resources
                if (_listener != null)
                {
                    _listener.Stop();
                }
            }
            disposed = true;
        }

        private void HandleClientRequest(TcpClient client)
        {
            if(!ReceiveRequest(client, out IRequest request))
            {
                return;
            }
            
            // logging
            StringBuilder str = new StringBuilder($"Server received operation from Client with ID: '{request.ClientId}'");
            str.Append($", revision: '{request.Revision}'");
            str.Append($", operations count: '{request.Operations.Count()}'");
            _logger.Log(str.ToString());
            
            // register client if not already registered.
            if (!_clients.ContainsKey(request.ClientId))
            {
                _clients.TryAdd(request.ClientId, client);
                _logger.Log($"Server has registered Client with ID: '{request.ClientId}'");
            }

            lock (_locker)
            {
                // transform request operations according to the current server state
                IRequest transformedRequest = TransformRequest(request);

                // apply operations over the server data document
                ApplyOperations(transformedRequest);
                        
                // save operations to the server revision log
                _revisionLog.TryAdd(_revision, transformedRequest.Operations.ToList());

                // acknowledge the request
                IRequest acknowledgedRequest = 
                    new Request(transformedRequest.ClientId, 
                        _revision + 1, 
                        transformedRequest.Operations.ToList(), 
                        true);
                        
                // broadcast operations to other clients
                Broadcast(acknowledgedRequest);
                        
                // increment local revision
                _revision++;
            }
        }

        private bool ReceiveRequest(TcpClient client, out IRequest request)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int count = stream.Read(buffer, 0, buffer.Length);
                byte[] formatted = new Byte[count];
                Array.Copy(buffer, formatted, count);
                request = (IRequest)Serializer.Deserialize(formatted);
                return true;
            }
            catch (Exception e)
            {
                // logging 
                _logger.Log($"Can't read message from client");

                // remove client from client list
                _clients.TryRemove(_clients.FirstOrDefault(c => c.Value == client));
                
                client.Close();
                request = null;
                return false;
            }

        }
        
        private void Broadcast(IRequest data)
        {
            foreach(var (id, client)  in _clients)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = Serializer.Serialize(data);
                stream.Write(buffer,0, buffer.Length);
                
                _logger.Log($"Message is sent by Server to Client with ID: '{id}'");
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