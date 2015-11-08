using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http
{
    enum Method { NONE = 0, GET, POST, PUT }

    class Request
    {
        public Method Method;

        public int ContentLength;

        #region Parsing storages

        internal bool IsComplete;

        internal byte[] Inputs;

        internal int InputsStartOffset;

        internal int InputsEndOffset;

        internal int State;

        internal byte[] Blobs = new byte[10000];

        /// <summary>
        /// Mapping from header to index into blobs offsets (reflects header ordering)
        /// </summary>
        internal readonly int[] BlobMapping = new int[10];

        internal readonly int[] BlobsOffsets = new int[10];

        #endregion
    }
}
