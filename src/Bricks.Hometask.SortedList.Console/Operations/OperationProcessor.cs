using Bricks.Hometask.Base;
using System.Collections.Generic;

namespace Bricks.Hometask.SortedList.Console
{
    public static class OperationProcessor
    {
        public static void InsertOperation(IList<int> data, IOperation operation)
        {            
            if (operation.Index > data.Count - 1)
            {
                // add if index is greater than data length
                data.Add(operation.Value.Value);
            }            
            else
            {
                // insert if index is in a range of data
                data.Insert(operation.Index, operation.Value.Value);
            }
        }

        public static void DeleteOperation(IList<int> data, IOperation operation)
        {
            data.RemoveAt(operation.Index);
        }
    }
}