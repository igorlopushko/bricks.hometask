using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bricks.Hometask.Runner
{
    public class Client
    {
        private IPAddress _ipAddress;
        private int _port;
        private TcpClient _tcpClient;
        private int _clientId;
        
        public Client(IPAddress ipAddress, int port, int clientId)
        {
            _ipAddress = ipAddress;
            _port = port;
            _clientId = clientId;
            
            _tcpClient = new TcpClient();
            _tcpClient.Connect(ipAddress, port);
            NetworkStream ns = _tcpClient.GetStream();

            Task.Run(() => ReceiveData(_tcpClient));
            
            while (true)
            {
                string s = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
                byte[] receivedBytes = new byte[1024];
                int byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);
                byte[] formated = new byte[byte_count];
                //handle  the null characteres in the byte array
                Array.Copy(receivedBytes, formated, byte_count); 
                string data = Encoding.ASCII.GetString(formated);
                Console.WriteLine(data);
            }
            
            ns.Close();
            _tcpClient.Close();
            Console.WriteLine("disconnect from server!!");
            Console.ReadKey();   
        }

        private void ReceiveData(TcpClient client)
        {
            
        }
    }
}
