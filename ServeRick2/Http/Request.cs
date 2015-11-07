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

        #region Parsing storages

        internal byte[] Inputs;

        internal int InputsStartOffset;

        internal int InputsEndOffset;

        internal int State;

        internal byte[] Blobs = new byte[10000];

        internal readonly int[] BlobsOffsets = new int[10];

        #endregion
    }
}
