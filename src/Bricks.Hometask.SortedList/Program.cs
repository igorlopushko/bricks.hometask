using Bricks.Hometask.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.SortedList.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // setup clients and number of operations
            int initDataCount = 30;
            int operationsCount = 10;
            int numberOfClients = 5;

            List<IClient> clients = new List<IClient>();
            List<Task> clientTasks = new List<Task>();
            Server server = new Server(Startup.ConfigurationRoot);

            // initialize initial state of the server data
            List<int> initData = new List<int>();
            for (int i = 0; i < initDataCount; i++)
            {
                initData.Add(i);
            }
            server.Initialize(initData);

            // run server
            Task.Run(() => server.Run());

            // run clients and execute operations
            for (int i = 0; i < numberOfClients; i++)
            {
                Client client = new Client(server, i + 1, Startup.ConfigurationRoot);
                clients.Add(client);

                server.RegisterClient(client);
                Task.Run(() => client.Run());
                Task t = Task.Run(() => PushOperationsToClient(client, operationsCount));

                clientTasks.Add(t);
            }

            if (bool.Parse(Startup.ConfigurationRoot["LoggingEnabled"]))
            {
                Task.WaitAll(clientTasks.ToArray());
            }

            while (true)
            {
                System.Console.Clear();

                System.Console.WriteLine($"Merge with sort algorithm".ToUpper());

                // print clients data
                foreach (IClient c in clients)
                {
                    PrintClient(c.ClientId, c.Data.ToArray());
                }

                PrintServer(server.Data.ToArray());

                Thread.Sleep(System.TimeSpan.FromSeconds(1));
            }


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

        private static void PrintClient(int clientId, int[] data)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Client with ID: '{clientId}' data set:");
            for (int i = 0; i < data.Length; i++)
            {
                if (i < data.Length - 1)
                    System.Console.Write(data[i] + " ");
                else
                    System.Console.Write(data[i]);
            }
            System.Console.WriteLine();
        }

        private static void PrintServer(int[] data)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Server data set:");
            for (int i = 0; i < data.Length; i++)
            {
                if (i < data.Length - 1)
                    System.Console.Write(data[i] + " ");
                else
                    System.Console.Write(data[i]);
            }
            System.Console.WriteLine();
        }
    }
}