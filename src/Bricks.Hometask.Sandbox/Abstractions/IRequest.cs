using System.Collections.Generic;

namespace Bricks.Hometask.Sandbox
{
    public interface IRequest
    {
        /// <summary>Gets collection of operations./// </summary>
        public IEnumerable<IOperation> Operations { get; }
        
        /// <summary>Gets client unique identifier that initiated the request./// </summary>
        public int ClientId { get; }
        
        /// <summary>Gets revision number that was used when request was initiated.</summary>
        public int Revision { get; }
        
        /// <summary>Gets value which determines whether operation(s) was acknowledged.</summary>
        public bool IsAcknowledged { get; }
    }
}