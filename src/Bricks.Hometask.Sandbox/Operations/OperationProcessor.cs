using System;
using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.Sandbox
{
    public static class OperationProcessor<T>
    {
        public static void InsertOperation(IList<T> data, IOperation<T> operation)
        {
            if (operation.Value == null)
            {
                string message = $"Can't insert NULL value with operation timestamp: {operation.Timestamp}";
                Log(message);
                throw new NullReferenceException(message);
            }

            if((operation.Index > -1 && operation.Index < data.Count()) || operation.Index == 0)
            {
                data.Insert(operation.Index, operation.Value);
            } 
            else 
            {
                //data.Insert(operation.Index - 1, operation.Value.Value);
                string message = $"Can't insert value: '{(operation.Value != null ? operation.Value : "NULL")}' " +
                                                      $"at index: '{operation.Index}', " +
                                                      $"operation timestamp: '{operation.Timestamp}'";
                Log(message);
                throw new ArgumentOutOfRangeException(message);
            }
        }

        public static void UpdateOperation(IList<T> data, IOperation<T> operation)
        {
            if (operation.Value == null)
            {
                string message = $"Can't update NULL value with operation timestamp: {operation.Timestamp}";
                Log(message);
                throw new NullReferenceException(message);
            }
            
            if (operation.Index < 0 || operation.Index > data.Count)
            {
                string message = $"Can't update element at index: '{operation.Index}' " +
                                 $"with value: '{(operation.Value != null ? operation.Value : "NULL")}', " +
                                 $"operation timestamp: '{operation.Timestamp}'";
                Log(message);
                throw new ArgumentOutOfRangeException(message);
            }
            
            data[operation.Index] = operation.Value;
        }
        
        public static void DeleteOperation(IList<T> data, IOperation<T> operation)
        {
            if (data.Count != 0 && operation.Index > -1 && operation.Index < data.Count)
            {
                data.RemoveAt(operation.Index);
            }
            else
            {
                //data.RemoveAt(operation.Index - 1);                
                string message = $"Can't delete item at index: '{operation.Index}', operation timestamp: '{operation.Timestamp}'";
                Log(message);
                throw new ArgumentOutOfRangeException(message);
            }
        }

        private static void Log(string text)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}