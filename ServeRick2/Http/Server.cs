using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol;

namespace ServeRick2.Http
{
    class Server
    {
        /// <summary>
        /// The pool for IO operations.
        /// </summary>
        private readonly IOPool _ioPool = new IOPool();

        /// <summary>
        /// Configuration of the server. Can be changed before server is started.
        /// </summary>
        internal PoolConfiguration Configuration { get { return _ioPool.Configuration; } }


    }
}
