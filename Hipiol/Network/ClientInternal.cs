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
        /// <summary>
        /// Event args object that is used for receiving events.
        /// </summary>
        internal readonly SocketAsyncEventArgs ReceiveEventArgs = new SocketAsyncEventArgs();

        /// <summary>
        /// Event args object that is used for sending events.
        /// </summary>
        internal readonly SocketAsyncEventArgs SendEventArgs = new SocketAsyncEventArgs();

        /// <summary>
        /// Actual buffer for receiving.
        /// </summary>
        internal Block ReceiveBuffer;

        /// <summary>
        /// Actual block which is sent.
        /// </summary>
        internal BlockChain ActualSendBlock;

        /// <summary>
        /// Block which was added last into the queue (the queue consists of linked blocks)
        /// </summary>
        internal BlockChain LastSendBlock;

        /// <summary>
        /// Tag assigned to currrent client.
        /// </summary>
        internal object Tag;

        /// <summary>
        /// Current client socket.
        /// </summary>
        internal Socket Socket;

        /// <summary>
        /// Time, when client has been firstly seen.
        /// </summary>
        internal DateTime ArrivalTime;

        /// <summary>
        /// Client identification.
        /// </summary>
        internal Client Client;

        /// <summary>
        /// Determine whether receiving is allowed for current client.
        /// </summary>
        internal bool AllowReceiving;

        internal ClientInternal(NetworkManager manager, int clientIndex)
        {
            Client = new Client(clientIndex, 0);

            ReceiveEventArgs.Completed += (o,e) => manager.Callback_Receive(this);
            SendEventArgs.Completed += (o, e) => manager.Callback_DataSent(this);
        }
    }
}
