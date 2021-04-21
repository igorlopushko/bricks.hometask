using System.Collections.Generic;
using System.Linq;

namespace Bricks.Hometask.Base
{
    public static class OperationTransformer
    {
        /// <summary>Performs transformation of operation set A to operation set B.</summary>
        /// <param name="a">Operation set A to be transformed.</param>
        /// <param name="b">Operation set B is a basis of the transformation.</param>
        /// <returns>Transformed operation set.</returns>
        public static IEnumerable<IOperation> Transform(IEnumerable<IOperation> a, IEnumerable<IOperation> b)
        {
            if (a.Count() == 0) return new List<IOperation>();
            List<IOperation> result = new List<IOperation>(a.ToArray());
            
            foreach (IOperation operationB in b)
            {
                List<IOperation> temp = new List<IOperation>();

                foreach (IOperation operationA in result)
                {
                    IOperation transformedOperation = null;
                    
                    if (operationA.OperationType == OperationType.Insert &&
                        operationB.OperationType == OperationType.Insert)
                    {
                        transformedOperation = TransformInsertInsert(operationA, operationB);
                    } else if (operationA.OperationType == OperationType.Insert &&
                               operationB.OperationType == OperationType.Delete)
                    {
                        transformedOperation = TransformInsertDelete(operationA, operationB);
                    } else if (operationA.OperationType == OperationType.Delete &&
                               operationB.OperationType == OperationType.Insert)
                    {
                        transformedOperation = TransformDeleteInsert(operationA, operationB);
                    } else if (operationA.OperationType == OperationType.Delete &&
                               operationB.OperationType == OperationType.Delete)
                    {
                        transformedOperation = TransformDeleteDelete(operationA, operationB);
                    }

                    if (transformedOperation != null)
                    {
                        temp.Add(transformedOperation);
                    }
                }

                result.Clear();
                result.AddRange(temp);
            }

            return result;
        }
        
        /// <summary>Transform Insert-Insert case.</summary>
        /// <param name="o1">An operation to be transformed.</param>
        /// <param name="o2">Operation with respect to which perform the transformation.</param>
        /// <returns>Transformed operation.</returns>
        private static IOperation TransformInsertInsert(IOperation o1, IOperation o2)
        {            
            if (o1.Index < o2.Index || (o1.Index == o2.Index && o1.Timestamp < o2.Timestamp))
            {
                // Tii(Ins[3, "a"], Ins[4, "b"]) -> Ins[3, "a"]
                return o1;
            }
            
            // Tii(Ins[3, "a"], Ins[1, "b"]) -> Ins[4, "a"]
            return OperationFactory.CreateOperation(o1.ClientId, o1.OperationType, o1.Index + 1, o1.Value.Value, o1.Timestamp);
        }

        /// <summary>Transform Insert-Delete case.</summary>
        /// <param name="o1">An operation to be transformed.</param>
        /// <param name="o2">Operation with respect to which perform the transformation.</param>
        /// <returns>Transformed operation.</returns>
        private static IOperation TransformInsertDelete(IOperation o1, IOperation o2)
        {            
            if (o1.Index <= o2.Index)
            {
                // Tid(Ins[3, "a"], Del[4]) -> Ins[3, "a"]
                return o1;
            }

            // Tid(Ins[3, "a"], Del[1]) -> Ins[2, "a"]
            return OperationFactory.CreateOperation(o1.ClientId, o1.OperationType, o1.Index - 1, o1.Value.Value, o1.Timestamp);
        }

        /// <summary>Transform Delete-Insert case.</summary>
        /// <param name="o1">An operation to be transformed.</param>
        /// <param name="o2">Operation with respect to which perform the transformation.</param>
        /// <returns>Transformed operation.</returns>
        private static IOperation TransformDeleteInsert(IOperation o1, IOperation o2)
        {
            if (o1.Index < o2.Index)
            {
                // Tdi(Del[3], Ins[4, "b"]) -> Del[3]
                return o1;
            }

            // Tdi(Del[3], Ins[1, "b"]) -> Del[4]
            return OperationFactory.CreateOperation(o1.ClientId, o1.OperationType, o1.Index + 1, o1.Value.Value, o1.Timestamp);
        }
        
        /// <summary>Transform Delete-Delete case.</summary>
        /// <param name="o1">An operation to be transformed.</param>
        /// <param name="o2">Operation with respect to which perform the transformation.</param>
        /// <returns>Transformed operation. NULL if operations are identical and operation execution is not needed.</returns>
        private static IOperation TransformDeleteDelete(IOperation o1, IOperation o2)
        {
            if (o1.Index < o2.Index)
            {
                // Tdd(Del[3], Del[4]) -> Del[3]
                return o1;
            }
            
            if (o1.Index > o2.Index)
            {
                // Tdd(Del[3], Del[1]) -> Del[2]
                return OperationFactory.CreateOperation(o1.ClientId, o1.OperationType, o1.Index - 1, o1.Value.Value, o1.Timestamp);
            }

            // breaking deletion (identity operation), meaning operations are equivalent Tdd(Del[3], Del[3]) -> I
            return null;
        }
    }
}