using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Hipiol.Network
{
    /// <summary>
    /// Internal representation of the client.
    /// </summary>
    struct ClientInternal
    {
        /// <summary>
        /// Key which ensures <see cref="Client"/> compatibility.
        /// </summary>
        internal int Key;

        internal Socket Socket;

        internal DateTime ArrivalTime;


    }
}
