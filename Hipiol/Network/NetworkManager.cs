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
        /// <summary>
        /// Pool assigned to current manager.
        /// </summary>
        private readonly IOPool _pool;

        /// <summary>
        /// Slots where internal representations of clients are stored.
        /// </summary>
        private readonly ClientInternal[] _clientSlots;

        /// <summary>
        /// Stack of free slots. Stack is used to be more cache friendly.
        /// </summary>
        private readonly Stack<int> _freeSlots;

        /// <summary>
        /// Free chains that can be used.
        /// </summary>
        private readonly Stack<BlockChain> _freeChains = new Stack<BlockChain>();

        internal NetworkManager(IOPool pool)
        {
            _pool = pool;
            _clientSlots = new ClientInternal[pool.Configuration.MaxClientCount];
            _freeSlots = new Stack<int>(_clientSlots.Length);

            for (var i = 0; i < _clientSlots.Length; ++i)
            {
                var clientInternal = new ClientInternal(this, i);
                _clientSlots[i] = clientInternal;
                _freeSlots.Push(i);
            }
        }

        /// <summary>
        /// Starts listening on given port.
        /// </summary>
        /// <param name="localPort">Port for listening.</param>
        internal void StartListening(int localPort)
        {
            // create the socket which listens for incoming connections
            var localEndPoint = new IPEndPoint(IPAddress.Any, localPort);

            startAcceptingClients(localEndPoint);
        }

        /// <summary>
        /// Gets <see cref="ClientInternal"/> that is identified by given client.
        /// </summary>
        /// <param name="client">Client which identifiest the <see cref="ClientInternal"/></param>
        /// <returns>The <see cref="ClientInternal"/>.</returns>
        internal ClientInternal GetClientInternal(Client client)
        {
            var clientInternal = _clientSlots[client.Index];
            if (clientInternal.Client.Key != client.Key)
                //client is not available yet
                return null;

            return clientInternal;
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

        /// <summary>
        /// Register new client, which is listening on given socket.
        /// </summary>
        /// <param name="socket">Socket where new client is connected.</param>
        /// <param name="arrivalTime">Time when client has been seen firstly.</param>
        /// <returns>Corresponding <see cref="ClientInternal"/> internal representation of the client.</returns>
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

        /// <summary>
        /// Starts receiving for given client and block. If timeout is specified, receiving will
        /// stop after that time.
        /// </summary>
        /// <param name="client">Client which receiving is set.</param>
        /// <param name="timeout">Timeout for receiving.</param>
        /// <param name="block">Block where received data will be stored.</param>
        internal void StartReceiving(ClientInternal client, int timeout, Block block)
        {
            if (client.AllowReceiving)
                throw new NotSupportedException("Cannot start receiving twice");

            if (timeout != 0)
                throw new NotImplementedException("Handle timeout by calendar");

            client.AllowReceiving = true;
            client.ReceiveBuffer = block;
            client.ReceiveEventArgs.SetBuffer(block.GetNativeBuffer(), 0, block.Size);

            RequestReceiving(client);
        }

        /// <summary>
        /// Expect that <see cref="ClientInternal.ReceiveBuffer"/> and <see cref="ClientInternal.ReceiveEventArgs"/> buffers are properly set.
        /// </summary>
        /// <param name="client">Client which receiving is requested.</param>
        internal void RequestReceiving(ClientInternal client)
        {
            if (!client.AllowReceiving)
                throw new NotSupportedException("Cannot continue receiving, when client doesn't allow receiving");

            if (!client.Socket.ReceiveAsync(client.ReceiveEventArgs))
            {
                //receive was handled synchronously
                Handle_Receive(client);
            }
        }

        /// <summary>
        /// Event args handler for data receiving.
        /// </summary>
        /// <param name="client">Client which received data.</param>
        internal void Handle_Receive(ClientInternal client)
        {
            _pool.Fire_DataReceive(client);
        }

        #endregion

        #region Data sending

        /// <summary>
        /// Sends given block to given client.
        /// </summary>
        /// <param name="client">Client whom the data will be sent.</param>
        /// <param name="block">Block that will be sent.</param>
        /// <param name="dataOffset">Offset where data in block starts.</param>
        /// <param name="dataSize">Size of data to send.</param>
        internal void Send(ClientInternal client, Block blockToSend, int dataOffset, int dataSize)
        {
            client.LastSendBlock = chainBlock(blockToSend, dataOffset, dataSize, client.LastSendBlock);

            if (client.ActualSendBlock != null)
                //there is nothing to do now - another block is sent right now.
                return;

            //there is only one block
            client.ActualSendBlock = client.LastSendBlock;

            //send buffer
            client.SendEventArgs.SetBuffer(blockToSend.GetNativeBuffer(), dataOffset, dataSize);


            if (!client.Socket.SendAsync(client.SendEventArgs))
            {
                //send was handled synchronously
                Handle_DataSent(client);
            }
        }

        /// <summary>
        /// Event args handler for data sent event.
        /// </summary>
        /// <param name="client">Client which data was sent.</param>
        internal void Handle_DataSent(ClientInternal client)
        {
            _pool.Fire_DataSent(client);
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Chains given block.
        /// </summary>
        /// <param name="block">Block to chain.</param>
        /// <param name="previousChain">Previous chain, where created chain will be appended.</param>
        /// <returns>Chain with given block.</returns>
        private BlockChain chainBlock(Block block, int dataOffset, int dataSize, BlockChain previousChain = null)
        {
            BlockChain freeChain;
            if (_freeChains.Count == 0)
                freeChain = new BlockChain();
            else
                freeChain = _freeChains.Pop();

            freeChain.Block = block;
            freeChain.DataOffset = dataOffset;
            freeChain.DataSize = dataSize;

            if (previousChain != null)
                //chain with previous chain if available
                previousChain.Next = freeChain;

            return freeChain;
        }

        /// <summary>
        /// Releases given <see cref="BlockChain"/>.
        /// </summary>
        /// <param name="chain">Released chain.</param>
        private void releaseChain(BlockChain chain)
        {
            chain.Block = null;
            chain.Next = null;
            chain.DataSize = 0;
            chain.DataOffset = 0;

            _freeChains.Push(chain);
        }

        #endregion
    }
}
