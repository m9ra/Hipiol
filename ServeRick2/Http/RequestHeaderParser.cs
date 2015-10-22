using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http
{
    class RequestHeaderParser
    {
        /// <summary>
        /// Determinine how long the request is.
        /// </summary>
        public readonly static Header<int> ContentLength;
        
        /// <summary>
        /// Accepts data in the given buffer.
        /// </summary>
        /// <param name="startOffset">Start of data to accept (inclusive).</param>
        /// <param name="endOffset">End of data to accept (exclusive).</param>
        /// <param name="buffer">Buffer with data.</param>
        internal void Accept(int startOffset, int endOffset, byte[] buffer)
        {
            for (var index = startOffset; index < endOffset; ++startOffset)
            {
                var acceptedByte = buffer[index];
                throw new NotImplementedException("Feed the state automaton");
            }
        }
    }
}
