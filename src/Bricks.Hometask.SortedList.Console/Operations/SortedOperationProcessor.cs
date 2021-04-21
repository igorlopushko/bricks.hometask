using Bricks.Hometask.Base;
using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.SortedList.Console
{
    public static class SortedOperationProcessor
    {
        public static IOperation InsertOperation(IList<int> data, IOperation operation)
        {
            // add if data is empty
            if(data.Count() == 0)
            {
                data.Add(operation.Value.Value);
                return OperationFactory.CreateOperation(operation.ClientId, OperationType.Insert, 0, operation.Value.Value, operation.Timestamp);
            }

            // perform binary search of the index to insert
            int index = FindInsertIndex(data.ToArray(), operation.Value.Value);
            data.Insert(index, operation.Value.Value);
            return OperationFactory.CreateOperation(operation.ClientId, OperationType.Insert, index, operation.Value.Value, operation.Timestamp);
        }

        public static IList<IOperation> UpdateOperation(IList<int> data, IOperation operation)
        {
            List<IOperation> result = new List<IOperation>();
            int value = data[operation.Index];
            data.RemoveAt(operation.Index);
            result.Add(OperationFactory.CreateOperation(operation.ClientId, OperationType.Delete, operation.Index, value, operation.Timestamp));

            int index = FindInsertIndex(data.ToArray(), operation.Value.Value);
            data.Insert(index, operation.Value.Value);
            result.Add(OperationFactory.CreateOperation(operation.ClientId, OperationType.Insert, index, operation.Value.Value, operation.Timestamp));

            return result;
        }
        
        public static IOperation DeleteOperation(IList<int> data, IOperation operation)
        {
            if (data.Count() == 0 || operation.Index >= data.Count)
            {
                return null;
            }

            int value = data[operation.Index];
            data.RemoveAt(operation.Index);
            return OperationFactory.CreateOperation(operation.ClientId, OperationType.Delete, operation.Index, value, operation.Timestamp);
        }

        private static int FindInsertIndex(int[] array, int key)
        {
            // lower and upper bounds
            int start = 0;
            int end = array.Length - 1;

            // traverse the search space
            while (start <= end)
            {
                int mid = (start + end) / 2;

                // if 'key' is found
                if (array[mid] == key)
                    return mid;
                else if (array[mid] < key)
                    start = mid + 1;
                else
                    end = mid - 1;
            }
            
            return end + 1;
        }
    }
}