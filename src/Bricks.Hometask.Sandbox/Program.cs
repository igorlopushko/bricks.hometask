using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            int operationsCount = 5;
            int numberOfClients = 3;
            List<IClient> clients = new List<IClient>();
            List<Task> clientTasks = new List<Task>();
            Server server = new Server();
            
            Task.Run(() => server.Run());

            for (int i = 0; i < numberOfClients; i++)
            {
                Client client = new Client(server, i + 1);
                clients.Add(client);

                server.RegisterClient(client);
                Task.Run(() => client.Run());
                Task t = Task.Run(() => PushOperationsToClient(client, operationsCount));
                
                clientTasks.Add(t);
            }

            Task.WaitAll(clientTasks.ToArray());
            server.Stop();

            //Task.WaitAll(serverTask);

            Console.WriteLine("Sync data");
            Thread.Sleep(System.TimeSpan.FromSeconds(1));
            
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
                IOperation operation = OperationRandomGenerator.GenerateRandomOperation(client);
                client.PushOperation(operation);

                // generate new operation 5 times per second
                Thread.Sleep(System.TimeSpan.FromMilliseconds(200));
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