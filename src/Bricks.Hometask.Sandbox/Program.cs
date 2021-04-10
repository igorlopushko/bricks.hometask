using System;
using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    public class Client
    {
        private List<int> _data;
        public int ClientId { get; private set; }
        public IEnumerable<int> Data
        {
            get
            {
                foreach (int product in _data)
                {
                    yield return product;
                }
            }
        }

        public Client(int clientId)
        {
            ClientId = clientId;
            _data = new List<int>();
        }

        public void LoadData()
        {
            //TODO: get initial data from server
        }
    }

    public class Operation
    {
        public OperationType OperationType { get; private set; }
        public int? Value { get; private set; }
        public int Index { get; private set; }

        public Operation(OperationType type, int index, int? value = null)
        {
            OperationType = type;
            Index = index;
            Value = value;
        }
    }

    public enum OperationType
    {
        Insert,
        Delete
    }

    public class Request
    {
        public int ClientId { get; private set; }
        //TODO: add state vector property
        public Operation Operation { get; private set; }
        //TODO: add operation priority property

        public Request(int clientId, Operation operation)
        {
            ClientId = clientId;
            Operation = operation;
        }
    }
}