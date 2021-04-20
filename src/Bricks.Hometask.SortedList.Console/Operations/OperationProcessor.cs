using Bricks.Hometask.Base;
using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.SortedList.Console
{
    public static class OperationProcessor
    {
        public static void InsertOperation(IList<int> data, IOperation operation)
        {
            // add if data is empty
            if (data.Count() == 0)
            {
                data.Add(operation.Value.Value);
            }

            // perform binary search of the index to insert
            int index = FindInsertIndex(data.ToArray(), operation.Value.Value);
            // add if index is greater than data length
            if (index > data.Count - 1)
            {
                data.Add(operation.Value.Value);
            }
            // insert if index is in a range of data
            else
            {
                data.Insert(index, operation.Value.Value);
            }
        }

        public static void DeleteOperation(IList<int> data, IOperation operation)
        {
            if (data.IndexOf(operation.Value.Value) != -1)
            {
                data.RemoveAt(data.IndexOf(operation.Value.Value));
            }
        }

        private static int FindInsertIndex(int[] array, int key)
        {
            if (array[0] > key) return 0;
            if (array[array.Length - 1] < key) return array.Length;

            int low = 0;
            int high = array.Length;
            int mid = 0;

            while (low <= high)
            {
                mid = (int)((uint)(low + high) >> 1);
                int midVal = array[mid];

                if (midVal < key)
                {
                    low = mid + 1;
                }
                else if (midVal > key)
                {
                    high = mid - 1;
                }
                else
                {
                    return mid + 1;
                }
            }

            return mid;
        }
    }
}