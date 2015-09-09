using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.Memory
{
    public class Block
    {
        /// <summary>
        /// Determine whether block data can be changed.
        /// </summary>
        public readonly bool IsConstant;

        /// <summary>
        /// Size of current block.
        /// </summary>
        public int Size { get { return _data.Length; } }

        /// <summary>
        /// Data storage of current <see cref="Block"/>
        /// </summary>
        private readonly byte[] _data;

        /// <summary>
        /// Initialize new <see cref="Block"/> with constant data.
        /// </summary>
        /// <param name="data">Data which will be copied to block's memory.</param>
        internal Block(byte[] data)
        {
            IsConstant = true;
            _data = data.ToArray();
        }

        internal Block(int size)
        {
            IsConstant = false;
            _data = new byte[size];
        }

        /// <summary>
        /// Gets buffer of current block. It provides fast access
        /// to the buffer, but has to be used carefully!!!
        /// </summary>
        /// <returns>The internal buffer.</returns>
        internal byte[] GetNativeBuffer()
        {
            return _data;
        }
    }
}
