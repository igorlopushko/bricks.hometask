using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            
            Task serverTask = Task.Run(() => server.Run());

            Client client1 = new Client(1);
            server.RegisterClient(client1);
            Task client1Task = Task.Run(() => client1.Run());
            
            Thread.Sleep(Timeout.TwoSeconds);
            
            Client client2 = new Client(2);
            server.RegisterClient(client2);
            Task client2Task = Task.Run(() => client2.Run());
            
            Thread.Sleep(Timeout.FiveSeconds);
            
            Client client3 = new Client(3);
            server.RegisterClient(client3);
            Task client3Task = Task.Run(() => client3.Run());
            
            /*Thread.Sleep(Timeout.FiveSeconds);
            server.UnregisterClient(client2);
            Thread.Sleep(Timeout.FiveSeconds);
            server.UnregisterClient(client1);
            Thread.Sleep(Timeout.FiveSeconds);
            server.UnregisterClient(client3);*/
            
            Console.ReadLine();
        }
    }

    
}