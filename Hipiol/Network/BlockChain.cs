using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;    

namespace Hipiol.Network
{
    /// <summary>
    /// Utility class for chaining <see cref="Block"/>. 
    /// <remarks>Blocks cannot be chained directly, because they can be shared.</remarks>
    /// </summary>
    class BlockChain
    {
        /// <summary>
        /// Pointer to next
        /// </summary>
        internal BlockChain Next;

        /// <summary>
        /// Chained block.
        /// </summary>
        internal Block Block;

        /// <summary>
        /// Where data to send starts in block.
        /// </summary>
        internal int DataOffset;

        /// <summary>
        /// How much of data should be sent from block
        /// </summary>
        internal int DataSize;
    }
}
