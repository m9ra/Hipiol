using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Hipiol.Memory;

namespace Hipiol.Network
{
    /// <summary>
    /// Internal representation of the client.
    /// </summary>
    class ClientInternal
    {
        internal Socket Socket;

        internal DateTime ArrivalTime;

        internal Client Client;

        internal readonly SocketAsyncEventArgs ReceiveEventArgs = new SocketAsyncEventArgs();

        internal Block ReceiveBuffer;

        internal bool IsReceiving;

        internal ClientInternal(NetworkManager manager, int clientIndex)
        {
            Client = new Client(clientIndex, 0);

            ReceiveEventArgs.Completed += (o,e) => manager.HandleReceive(this);
        }
    }
}
