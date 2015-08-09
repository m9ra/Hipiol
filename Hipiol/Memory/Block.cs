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
    }
}
