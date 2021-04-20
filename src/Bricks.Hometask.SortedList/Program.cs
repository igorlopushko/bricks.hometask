using Bricks.Hometask.Base;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.SortedList.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // setup clients and number of operations
            int operationsCount = 5;
            int numberOfClients = 3;

            List<IClient> clients = new List<IClient>();
            List<Task> clientTasks = new List<Task>();
            Server server = new Server();

            // initialize initial state of the server data
            List<int> initData = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                initData.Add(i);
            }
            server.Initialize(initData);

            // run server
            Task.Run(() => server.Run());

            // run clients and execute operations
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

            /*
            clientTasks.Clear();
            foreach (IClient c in clients)
            {
                clientTasks.Add(Task.Run(() => c.Stop()));
            }
            Task.WaitAll(clientTasks.ToArray());
            */

            // sleep to sync all the data
            Thread.Sleep(System.TimeSpan.FromSeconds(20));
            //server.Stop();

            // print clients data
            foreach (IClient c in clients)
            {
                PrintClient(c);
            }

            PrintServer(server);

            System.Console.ReadLine();
        }

        private static void PushOperationsToClient(IClient client, int operationsCount)
        {
            for (int i = 0; i < operationsCount; i++)
            {
                // generate new operation 5 times per second
                Thread.Sleep(System.TimeSpan.FromMilliseconds(200));

                IOperation operation = OperationRandomGenerator.GenerateRandomOperation(client);
                client.PushOperation(operation);
            }

            System.Console.WriteLine($"Client '{client.ClientId}' finished processing");
        }

        private static void PrintClient(IClient client)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Client with ID: '{client.ClientId}' data set:");
            foreach (int d in client.Data)
            {
                System.Console.WriteLine(d);
            }
            System.Console.WriteLine();
        }

        private static void PrintServer(IServer server)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Server data set:");
            foreach (int d in server.Data)
            {
                System.Console.WriteLine(d);
            }
            System.Console.WriteLine();
        }
    }
}
