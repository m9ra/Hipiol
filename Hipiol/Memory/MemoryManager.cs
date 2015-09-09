using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.Memory
{
    class MemoryManager
    {
        /// <summary>
        /// Configuration of current manager.
        /// </summary>
        private readonly PoolConfiguration _configuration;

        /// <summary>
        /// Index of constant blocks.
        /// </summary>
        private readonly List<Block> _constantBlocks=new List<Block>();

        /// <summary>
        /// Index of dynamic blocks.
        /// </summary>
        private readonly List<Block> _dynamicBlocks = new List<Block>();

        /// <summary>
        /// How many bytes has been allocated for constant blocks.
        /// </summary>
        private int _constantBytesAllocated = 0;

        /// <summary>
        /// How many bytes has been allocated for IO blocks.
        /// </summary>
        private int _ioBytesAllocated = 0;

        internal MemoryManager(PoolConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Creates block from copy of given data.
        /// </summary>
        /// <param name="data">Data that will be copied into the created block.</param>
        /// <returns>The created block.</returns>
        internal Block CreateConstantBlock(byte[] data)
        {
            if (_constantBytesAllocated + data.Length > _configuration.ConstantMemoryLimit)
                throw new NotSupportedException("Cannot allocate more data due to configuration limit for constant blocks");
            
            var block = new Block(data);
            _constantBlocks.Add(block);

            //register allocated memory
            _constantBytesAllocated += data.Length; 

            return block;
        }

        /// <summary>
        /// Get free memory block.
        /// </summary>
        /// <returns>Memory block.</returns>
        internal Block GetIOBlock()
        {
            //TODO blocks pool!!!
            _ioBytesAllocated += _configuration.IOBlockSize;
            return new Block(_configuration.IOBlockSize);
        }
    }
}
