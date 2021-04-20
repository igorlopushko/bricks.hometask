using Bricks.Hometask.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.OperationTransformation.Console
{
    public static class OperationProcessor
    {
        public static void InsertOperation(IList<int> data, IOperation operation)
        {
            if (!operation.Value.HasValue)
            {
                string message = $"Can't insert NULL value with operation timestamp: {operation.Timestamp}";
                Log(message);
                throw new NullReferenceException(message);
            }
            
            if (operation.Index == data.Count())
            {
                data.Add(operation.Value.Value);
            }
            else if (operation.Index > -1 && operation.Index < data.Count())
            {
                data.Insert(operation.Index, operation.Value.Value);
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

        public static void UpdateOperation(IList<int> data, IOperation operation)
        {
            if (!operation.Value.HasValue)
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
            
            data[operation.Index] = operation.Value.Value;
        }
        
        public static void DeleteOperation(IList<int> data, IOperation operation)
        {
            // Skip delete if data is empty. It might happen update occured in another client and transformed to Delete/Insert.
            if (data.Count == 0 || operation.Index > data.Count() - 1) return;

            if (data.Count != 0 && operation.Index > -1 && operation.Index < data.Count)
            {
                data.RemoveAt(operation.Index);
            }
            else
            {
                string message = $"Can't delete item at index: '{operation.Index}', operation timestamp: '{operation.Timestamp}'";
                Log(message);
                throw new ArgumentOutOfRangeException(message);
            }
        }

        private static void Log(string text)
        {            
            System.Console.WriteLine(text);
        }
    }
}