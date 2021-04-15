using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            int operationsCount = 10;
            List<Task> tasks = new List<Task>();
            Server server = new Server();
            
            Task serverTask = Task.Run(() => server.Run());

            //TODO: add server alive check before register new client
            Client client1 = new Client(1);
            server.RegisterClient(client1);
            Task client1Task = Task.Run(() => client1.Run());
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < operationsCount; i++)
                {
                    IOperation operation = OperationRandomGenerator.GenerateRandomOperation(client1.Data.Count());
                    client1.PushOperation(operation);
                    
                    //TODO: add some random delay
                    Thread.Sleep(RandomGenerator.GetDelay(100, 3000));
                }
            }));
            
            Thread.Sleep(Timeout.TwoSeconds);
            
            //TODO: add server alive check before register new client
            Client client2 = new Client(2);
            server.RegisterClient(client2);
            Task client2Task = Task.Run(() => client2.Run());
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < operationsCount; i++)
                {
                    IOperation operation = OperationRandomGenerator.GenerateRandomOperation(client2.Data.Count());
                    client2.PushOperation(operation);
                    
                    //TODO: add some random delay
                    Thread.Sleep(RandomGenerator.GetDelay(300, 500));
                }
            }));
            
            
            Thread.Sleep(Timeout.FiveSeconds);
            
            //TODO: add server alive check before register new client
            Client client3 = new Client(3);
            server.RegisterClient(client3);
            Task client3Task = Task.Run(() => client3.Run());
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < operationsCount; i++)
                {
                    IOperation operation = OperationRandomGenerator.GenerateRandomOperation(client3.Data.Count());
                    client3.PushOperation(operation);
                    
                    //TODO: add some random delay
                    Thread.Sleep(RandomGenerator.GetDelay(250, 750));
                }
            }));

            Task.WaitAll(tasks.ToArray());
            
            client1.Stop();
            server.UnregisterClient(client1);
            client2.Stop();
            server.UnregisterClient(client2);
            client3.Stop();
            server.UnregisterClient(client3);
            
            Console.WriteLine();
            Console.WriteLine("Client 1:");
            foreach (var d in client1.Data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Client 2:");
            foreach (var d in client2.Data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Client 3:");
            foreach (var d in client3.Data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine();
            
            Console.WriteLine("Server is ready to be stopped");
            server.Stop();
            
            Console.ReadLine();
        }
    }
}