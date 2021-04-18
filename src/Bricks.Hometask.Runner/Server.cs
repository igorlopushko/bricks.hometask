using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Bricks.Hometask.Runner
{
    public class Server
    {
        private readonly TcpListener _listener;
        private int _port;

        public Server(int port) 
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
                        
            while (true)
            {                
                TcpClient client = _listener.AcceptTcpClient();
                Task.Run(() => HandleClientRequest(client));
            }
        }

        private void HandleClientRequest(TcpClient client)
        {

        }
    }
}
