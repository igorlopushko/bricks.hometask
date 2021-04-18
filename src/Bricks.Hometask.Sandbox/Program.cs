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
            int operationsCount = 5;
            int numberOfClients = 2;
            List<IClient> clients = new List<IClient>();
            List<Task> clientTasks = new List<Task>();
            Server server = new Server();
            
            Task serverTask = Task.Run(() => server.Run());
            //tasks.Add(serverTask);

            // spin till the server is up and running
            if (!server.IsAlive)
            {
                Console.WriteLine();
                Console.WriteLine("Wait till the Server is up and running");

                while(!server.IsAlive)
                {
                    Console.WriteLine(".");
                    Thread.Sleep(10);
                }
                Console.WriteLine();
            }

            for (int i = 0; i < numberOfClients; i++)
            {
                Client client = new Client(i + 1);
                clients.Add(client);

                server.RegisterClient(client);
                Task.Run(() => client.Run());
                Task t = Task.Run(() => PushOperationsToClient(client, operationsCount));
                
                clientTasks.Add(t);
            }

            Task.WaitAll(clientTasks.ToArray());

            //server.Stop();

            //Task.WaitAll(serverTask);

            Console.WriteLine("Sync data");
            Thread.Sleep(System.TimeSpan.FromSeconds(5));

            /*
            foreach (IClient c in clients)
            {
                c.Stop();
            }
            */

            foreach (IClient c in clients)
            {
                PrintClient(c);
            }

            PrintServer(server);

            Console.ReadLine();
        }

        private static void PushOperationsToClient(IClient client, int operationsCount)
        {            
            for (int i = 0; i < operationsCount; i++)
            {
                IOperation operation = OperationRandomGenerator.GenerateRandomOperation(client.Data.Count());
                client.PushOperation(operation);

                Thread.Sleep(Timeout.ClientOperationDelay);                
            }

            Console.WriteLine($"Client '{client.ClientId}' finished processing");
        }

        private static void PrintClient(IClient client)
        {
            Console.WriteLine();
            Console.WriteLine($"Client with ID: '{client.ClientId}' data set:");
            foreach (int d in client.Data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine();
        }

        private static void PrintServer(IServer server)
        {
            Console.WriteLine();
            Console.WriteLine($"Server data set:");
            foreach (int d in server.Data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine();
        }
    }
}