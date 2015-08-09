using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Hipiol.Events;

namespace Hipiol.Network
{
    class NetworkManager
    {
        private readonly EventChannelBase<EventBase> _channel;

        private readonly PoolConfiguration _configuration;

        internal NetworkManager(EventChannelBase<EventBase> channel, PoolConfiguration configuration)
        {
            _channel = channel;
            _configuration = configuration;
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
            listenSocket.Listen(_configuration.AcceptBacklog);

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
            throw new NotImplementedException();
        }

        #endregion
    }
}
