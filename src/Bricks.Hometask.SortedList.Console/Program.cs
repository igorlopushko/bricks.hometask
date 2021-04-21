using Bricks.Hometask.Base;
using Bricks.Hometask.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.SortedList.Console
{
    class Program
    {
        private static bool _logginEnabled;
        private static bool _revisionLogEnabled;

        static void Main(string[] args)
        {
            _logginEnabled = bool.Parse(Startup.ConfigurationRoot["LoggingEnabled"]);
            _revisionLogEnabled = bool.Parse(Startup.ConfigurationRoot["RevisionLogEnabled"]);

            // setup number of clients and operations, initial data set length
            int initDataCount = 10;
            int operationsCount = 5;
            int numberOfClients = 2;

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

            if (_logginEnabled)
            {
                Task.WaitAll(clientTasks.ToArray());
            }

            while (true)
            {
                if (!_logginEnabled)
                {
                    System.Console.Clear();
                }

                System.Console.WriteLine();
                System.Console.WriteLine($"Merge with sort algorithm".ToUpper());

                // print clients data
                foreach (IClient c in clients)
                {
                    PrintClient(c.ClientId, c.Data.ToArray(), server.Data.ToArray());
                }

                PrintServer(server.Data.ToArray());
                if(_revisionLogEnabled) PrintRevisionLog(server.RevisionLog);

                Thread.Sleep(System.TimeSpan.FromSeconds(1));
            }
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

            if (_logginEnabled) System.Console.WriteLine($"Client '{client.ClientId}' finished processing");
        }

        private static void PrintClient(int clientId, int[] data, int[] serverData)
        {
            ConsoleLogger logger = new ConsoleLogger(System.ConsoleColor.Red);
            System.Console.WriteLine();
            System.Console.WriteLine($"Client with ID: '{clientId}' data set:");
            for (int i = 0; i < data.Length; i++)
            {
                if(i > serverData.Length - 1 || data[i] != serverData[i])
                {
                    // print error value
                    logger.LogWrite(string.Format("{0,3}", data[i]));
                }
                else
                {
                    // print valid value
                    System.Console.Write(string.Format("{0,3}", data[i]));
                }

                // print delimeter
                if (i < data.Length - 1) System.Console.Write(" ");
            }
            System.Console.WriteLine();
        }

        private static void PrintServer(int[] data)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Server data set:");            
            for (int i = 0; i < data.Length; i++)
            {
                StringBuilder text = new StringBuilder();
                text.Append(string.Format("{0,3}", data[i]));

                if (i < data.Length - 1) text.Append(' '); 

                System.Console.Write(text.ToString());
            }

            // print indexes
            System.Console.WriteLine();
            ConsoleLogger logger = new ConsoleLogger(System.ConsoleColor.Red);
            for (int i = 0; i < data.Length; i++)
            {                
                StringBuilder text = new StringBuilder();
                text.Append(string.Format("{0,3}", i));

                if (i < data.Length - 1) text.Append(' ');

                logger.LogWrite(text.ToString());
            }

            System.Console.WriteLine();
        }

        private static void PrintRevisionLog(IDictionary<int, IList<IOperation>> revisionLog)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Server revision log:");
            foreach (var item in revisionLog)
            {
                System.Console.WriteLine($"Revision: #{item.Key}, Operataions: ");
                foreach (var operation in item.Value)
                {
                    StringBuilder text = new StringBuilder($"ClientId: '{operation.ClientId}', Type: '{operation.OperationType}', Index: '{operation.Index}'");                    
                    if (operation.OperationType == OperationType.Insert)
                    {
                        text.Append($", Value: '{operation.Value}'");
                    }
                    System.Console.WriteLine(text.ToString());                    
                }
                System.Console.WriteLine();
            }
        }
    }
}