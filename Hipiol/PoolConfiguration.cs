using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol
{
    /// <summary>
    /// Configuration of IO pool.
    /// </summary>
    public class PoolConfiguration
    {
        /// <summary>
        /// Number of bytes in megabyte.
        /// </summary>
        public static readonly int MB = (int)Math.Pow(2, 20);

        /// <summary>
        /// Allocation limit for constant memory blocks.
        /// </summary>
        public readonly int ConstantMemoryLimit = 10 * MB;

        /// <summary>
        /// Allocation limit for dynamic memory blocks.
        /// </summary>
        public readonly int DynamicMemoryLimit = 10 * MB;

        /// <summary>
        /// Maximum count of network clients that can be handled in parallel.
        /// </summary>
        public int MaxClientCount = 10000;

        /// <summary>
        /// How many clients in accept backlog could be stored.
        /// </summary>
        public int AcceptBacklog = 1024;

        /// <summary>
        /// Determine whether configuration is frozen (unable to change) or not.
        /// </summary>
        private bool _isFrozen = false;

        /// <summary>
        /// Makes current configuration frozen (unable to change).
        /// </summary>
        internal void Freeze()
        {
            _isFrozen = true;
        }
    }
}
