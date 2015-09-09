using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Hipiol.Events;
using Hipiol.Memory;

namespace Hipiol.Network
{
    /// <summary>
    /// Firstly, client is accepted -> registered -> active -> disposed.
    /// </summary>
    class NetworkManager
    {
        private readonly IOPool _pool;

        private readonly ClientInternal[] _clientSlots;

        private readonly Stack<int> _freeSlots;

        internal NetworkManager(IOPool pool)
        {
            _pool = pool;
            _clientSlots = new ClientInternal[pool.Configuration.MaxClientCount];
            _freeSlots = new Stack<int>(_clientSlots.Length);

            for (var i = 0; i < _clientSlots.Length; ++i)
            {
                var clientInternal = new ClientInternal(this,i);
                _clientSlots[i] = clientInternal;
                _freeSlots.Push(i);
            }
        }

        internal void StartListening(int localPort)
        {
            // create the socket which listens for incoming connections
            var localEndPoint = new IPEndPoint(IPAddress.Any, localPort);

            startAcceptingClients(localEndPoint);
        }

        #region Client accepting

        /// <summary>
        /// Starts accepting clients on given end point.
        /// </summary>
        /// <param name="endPoint">Local endpoint where clients will be accepted.</param>
        private void startAcceptingClients(IPEndPoint endPoint)
        {
            var listenSocket = new
                    Socket(endPoint.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

            //set listening socket
            listenSocket.Bind(endPoint);
            listenSocket.Listen(_pool.Configuration.AcceptBacklog);

            //create event args object shared for all AcceptAsync calls
            var e = createAcceptEventArgs();
            if (!listenSocket.AcceptAsync(e))
                //we are accepting client immediately
                //therefore synchronous call is required
                _acceptCallback(listenSocket, e);
        }

        /// <summary>
        /// Creates event args object with properties set for <see cref="_acceptCallback"/>.
        /// </summary>
        /// <returns>Created event args object.</returns>
        private SocketAsyncEventArgs createAcceptEventArgs()
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += _acceptCallback;
            return args;
        }

        /// <summary>
        /// Callback that is called after 
        /// </summary>
        /// <param name="sender">Listening socket which is accepting clients.</param>
        /// <param name="e">Event args processed by <see cref="AcceptAsync"/> call.</param>
        private void _acceptCallback(object sender, SocketAsyncEventArgs e)
        {
            var listenSocket = (Socket)sender;
            do
            {
                try
                {
                    acceptClient(e);
                }
                finally
                {
                    //clear socket, so the e object can be reused.
                    e.AcceptSocket = null;
                }
            } while (!listenSocket.AcceptAsync(e));
        }

        /// <summary>
        /// Accepts client described in <see cref="e"/>. 
        /// <remarks>Notice that <see cref="e"/> will be reused after current call</remarks>         
        /// </summary>
        /// <param name="e">Description of client to accept.</param>
        private void acceptClient(SocketAsyncEventArgs e)
        {
            switch (e.SocketError)
            {
                case SocketError.Success:
                    _pool.Fire_AcceptClient(e.AcceptSocket);
                    break;
                default:
                    throw new NotSupportedException("Cannot handle code " + e.SocketError);
            }
        }

        #endregion

        #region Client registration

        internal ClientInternal RegisterClient(Socket socket, DateTime arrivalTime)
        {
            if (_freeSlots.Count < 0)
                throw new NotImplementedException("Limit for count of clients has been reached");

            if (socket == null)
                throw new ArgumentNullException("socket");

            var freeSlot = _freeSlots.Pop();

            //key for slot is set when client is disposed - because of preventing operations on disposed clients
            var client = _clientSlots[freeSlot];
            client.Socket = socket;
            client.ArrivalTime = arrivalTime;

            return client;
        }

        #endregion

        #region Data receiving

        internal void StartReceiving(ClientInternal client, int timeout, Block block)
        {
            if (client.IsReceiving)
                throw new NotSupportedException("Cannot start receiving twice");

            if (timeout != 0)
                throw new NotImplementedException("Handle timeout by calendar");

            client.IsReceiving = true;
            client.ReceiveBuffer = block;
            client.ReceiveEventArgs.SetBuffer(block.GetNativeBuffer(), 0, block.Size);
            if (client.Socket.ReceiveAsync(client.ReceiveEventArgs))
            {
                //receive was handled synchronously
                HandleReceive(client);
            }
        }

        internal void HandleReceive(ClientInternal client)
        {
            _pool.Fire_DataReceive(client);
        }
        #endregion
    }
}
