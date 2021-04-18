using System;
using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.Sandbox
{
    public static class OperationProcessor
    {
        public static void InsertOperation(IList<int> data, IOperation operation)
        {
            if (!operation.Value.HasValue)
            {
                throw new NullReferenceException($"Can't insert NULL value with operation timestamp: {operation.Timestamp}");
            }

            if (operation.Index < 0 || operation.Index > data.Count())
            {
                throw new ArgumentOutOfRangeException($"Can't insert value: '{(operation.Value.HasValue ? operation.Value.Value : "NULL")}' " +
                                                      $"at index: '{operation.Index}', " +
                                                      $"operation timestamp: '{operation.Timestamp}'");
            }
            
            data.Insert(operation.Index, operation.Value.Value);
        }

        public static void UpdateOperation(IList<int> data, IOperation operation)
        {
            if (!operation.Value.HasValue)
            {
                throw new NullReferenceException($"Can't update NULL value with operation timestamp: {operation.Timestamp}");
            }
            
            if (operation.Index < 0 || operation.Index > data.Count)
            {
                throw new ArgumentOutOfRangeException($"Can't update element at index: '{operation.Index}' " +
                                                      $"with value: '{(operation.Value.HasValue ? operation.Value.Value : "NULL")}', " +
                                                      $"operation timestamp: '{operation.Timestamp}'");
            }
            
            data[operation.Index] = operation.Value.Value;
        }
        
        public static void DeleteOperation(IList<int> data, IOperation operation)
        {
            if (data.Count == 0 || operation.Index < 0 || operation.Index >= data.Count)
            {
                throw new ArgumentOutOfRangeException($"Can't delete item at index: '{operation.Index}', " +
                                                      $"operation timestamp: '{operation.Timestamp}'");
            }
            data.RemoveAt(operation.Index);
        }
    }
}