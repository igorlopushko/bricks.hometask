﻿using Bricks.Hometask.Base;
using Bricks.Hometask.Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bricks.Hometask.OperationTransformation.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // setup clients and number of operations
            int operationsCount = 5;
            int numberOfClients = 10;

            List<IClient<int>> clients = new List<IClient<int>>();
            List<Task> clientTasks = new List<Task>();
            Server<int> server = new Server<int>();
            
            Task.Run(() => server.Run());

            // run clients and execute operations
            for (int i = 0; i < numberOfClients; i++)
            {
                Client<int> client = new Client<int>(server, i + 1);
                clients.Add(client);

                server.RegisterClient(client);
                Task.Run(() => client.Run());
                Task t = Task.Run(() => PushOperationsToClient(client, operationsCount));
                
                clientTasks.Add(t);
            }

            Task.WaitAll(clientTasks.ToArray());            
            
            clientTasks.Clear();
            foreach (IClient<int> c in clients)
            {
                clientTasks.Add(Task.Run(() => c.Stop()));
            }
            Task.WaitAll(clientTasks.ToArray());            

            // sleep to sync all the data
            Thread.Sleep(System.TimeSpan.FromSeconds(20));
            server.Stop();

            // print clients data
            foreach (IClient<int> c in clients)
            {
                PrintClient(c);
            }

            PrintServer(server);

            System.Console.ReadLine();
        }

        private static void PushOperationsToClient(IClient<int> client, int operationsCount)
        {            
            for (int i = 0; i < operationsCount; i++)
            {
                // generate new operation 5 times per second
                Thread.Sleep(System.TimeSpan.FromMilliseconds(200));

                IOperation<int> operation = OperationRandomGenerator.GenerateRandomOperation(client);
                client.PushOperation(operation);
            }

            System.Console.WriteLine($"Client '{client.ClientId}' finished processing");
        }

        private static void PrintClient(IClient<int> client)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Client with ID: '{client.ClientId}' data set:");
            foreach (int d in client.Data)
            {
                System.Console.WriteLine(d);
            }
            System.Console.WriteLine();
        }

        private static void PrintServer(IServer<int> server)
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