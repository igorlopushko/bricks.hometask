using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public delegate void OperationSentEventHandler(Request request);
    
    public interface IClient
    {
        public int ClientId { get; }
        public IEnumerable<int> Data { get; }
        public void InitData(IEnumerable<int> data, int revision);
        public void Run();
        public void Stop();
        
        public event OperationSentEventHandler OperationSent;
    }
}