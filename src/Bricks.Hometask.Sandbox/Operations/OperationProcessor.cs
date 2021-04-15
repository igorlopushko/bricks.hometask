using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Bricks.Hometask.Sandbox
{
    public static class OperationProcessor
    {
        private static readonly ReaderWriterLockSlim SlimLocker = new ReaderWriterLockSlim();
        
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

            SlimLocker.EnterWriteLock();
            try
            {
                data.Insert(operation.Index, operation.Value.Value);
            }
            finally
            {
                SlimLocker.ExitWriteLock();
            }
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
            
            SlimLocker.EnterWriteLock();
            try
            {
                data[operation.Index] = operation.Value.Value;
            }
            finally
            {
                SlimLocker.ExitWriteLock();
            }
        }
        
        public static void DeleteOperation(IList<int> data,IOperation operation)
        {
            if (data.Count == 0 || operation.Index < 0 || operation.Index >= data.Count)
            {
                throw new ArgumentOutOfRangeException($"Can't insert value: '{(operation.Value.HasValue ? operation.Value.Value : "NULL")}' " +
                                                      $"at index: '{operation.Index}', " +
                                                      $"operation timestamp: '{operation.Timestamp}'");
            }

            SlimLocker.EnterWriteLock();
            try
            {
                data.RemoveAt(operation.Index);
            }
            finally
            {
                SlimLocker.ExitWriteLock();
            }
        }
    }
}