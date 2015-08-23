using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net.Sockets;

using Hipiol.Memory;
using Hipiol.Events;
using Hipiol.Network;

namespace Hipiol
{
    /// <summary>
    /// Delegate used for client accepted events.
    /// <remarks>Given client belongs to a different object every time.</remarks>
    /// </summary>
    /// <param name="client">Client which was accepted</param>
    public delegate void ClientAccepted(Client client);

    /// <summary>
    /// Delegate used for client disconnected events.
    /// <remarks>Disconnected event is raised just once for every accepted client.</remarks>
    /// </summary>
    /// <param name="client">Client which was disconnected.</param>
    public delegate void ClientDisconnected(Client client);

    /// <summary>
    /// Delegate used for data received events.
    /// </summary>
    /// <param name="client">Client which received the data.</param>
    /// <param name="block">Block where received data are stored.</param>
    public delegate void DataReceived(Client client, Block block);

    /// <summary>
    /// Delegate used for data sent completition events.
    /// </summary>
    /// <param name="client">Client which data block sending was completed.</param>
    public delegate void DataBlockSent(Client client);

    /// <summary>
    /// Represents a pool which provides lightweight access to disk and network IO. 
    /// All callbacks are handled by a single thread. 
    /// Commands for a network client are processed in synchronized order, however ordering of between different clients is not determined.
    /// </summary>
    public class IOPool
    {
        /// <summary>
        /// Current configuration of the pool. After initialization routines are called, configuration cannot be changed.
        /// </summary>
        public readonly PoolConfiguration Configuration;

        #region Event storages

        /// <summary>
        /// Storage for accepted events.
        /// </summary>
        private readonly EventStorage<ClientAcceptedEvent> _clientAcceptedEvents = new EventStorage<ClientAcceptedEvent>();

        #endregion

        /// <summary>
        /// Manager thata handleas network communication.
        /// </summary>
        internal NetworkManager Network { get; private set; }

        /// <summary>
        /// Here are processed all events of the pool.
        /// </summary>
        private readonly IOChannel _iochannel;

        /// <summary>
        /// Thread which processes events for _iochannel.
        /// </summary>
        private readonly Thread _eventThread;

        /// <summary>
        /// Manager that handles memory blocks.
        /// </summary>
        private MemoryManager _memory;

        private ClientAccepted _clientAcceptedHandler;

        private ClientDisconnected _clientDisconnectedHandler;

        private DataReceived _dataReceivedHandler;

        private DataBlockSent _dataBlockSentHandler;

        public IOPool()
        {
            //set default configuration
            Configuration = new PoolConfiguration();

            _iochannel = new IOChannel(this);

            _eventThread = new Thread(_iochannel.ProcessEvents);
            _eventThread.Start();
        }

        public void Send(Client client, Block block)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enables receiving for given client.
        /// <remarks>Data receiving is not enabled for accepted clients by default.</remarks>
        /// </summary>
        /// <param name="client">Client which receiving will be set.</param>
        /// <param name="timeout">Timeout, which is in milliseconds, will fire data received event with an <c>null</c> block after expiration. Zero timeout will wait for infinity. Negative timeout is an error.</param>
        public void AllowReceive(Client client, int timeout = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disconnects given client from the current pool.
        /// </summary>
        /// <param name="client">Client which will be disconnected.</param>
        public void Disconnect(Client client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts listening to TCP/IP connections on given local port.
        /// <remarks>Every accepted client will be reported through the client accepted handler. Closing of the communication with the    client is reported through the disconnection handler, which is called just once for every client.</remarks>
        /// </summary>
        /// <param name="localPort">Number of given local port where listening will be started.</param>
        public void StartListening(int localPort)
        {
            if (_clientAcceptedHandler == null || _clientDisconnectedHandler == null)
                throw new NotSupportedException("Cannot start listening without client handlers.");

            if (_dataReceivedHandler == null || _dataBlockSentHandler == null)
                throw new NotSupportedException("Cannot start listening without data handlers.");

            if (Network != null)
                throw new NotSupportedException("Cannot start listening twice.");

            Configuration.Freeze();
            Network = new NetworkManager(this);
            Network.StartListening(localPort);
        }

        /// <summary>
        /// Sets handlers for client acceptance and disconnections.
        /// </summary>
        /// <param name="clientAcceptedHandler">Accepted clients are reported by this handler.</param>
        /// <param name="clientDisconnectedHandler">Client disconnection is reported by this handler.</param>
        public void SetClientHandlers(ClientAccepted clientAcceptedHandler, ClientDisconnected clientDisconnectedHandler)
        {
            if (clientAcceptedHandler == null)
                throw new ArgumentNullException("clientAcceptedHandler");

            if (clientDisconnectedHandler == null)
                throw new ArgumentNullException("clientDisconnectedHandler");

            if (_clientAcceptedHandler != null || _clientDisconnectedHandler != null)
                throw new NotSupportedException("Cannot change client handlers.");

            _clientAcceptedHandler = clientAcceptedHandler;
            _clientDisconnectedHandler = clientDisconnectedHandler;
        }

        /// <summary>
        /// Set handlers for network data manipulation.
        /// </summary>
        /// <param name="dataReceivedHandler">Received data are reported by this handler.</param>
        /// <param name="dataBlockSentHandler">Successful completition of data block sending is reporeted by this handler. It is called just once for every <see cref="Send"/> if sending is successful.</param>
        public void SetDataHandlers(DataReceived dataReceivedHandler, DataBlockSent dataBlockSentHandler)
        {
            if (dataReceivedHandler == null)
                throw new ArgumentNullException("dataReceivedHandler");

            if (dataBlockSentHandler == null)
                throw new ArgumentNullException("dataBlockSentHandler");

            if (_dataReceivedHandler != null || _dataBlockSentHandler != null)
                throw new NotSupportedException("Cannot change data handlers.");

            _dataReceivedHandler = dataReceivedHandler;
            _dataBlockSentHandler = dataBlockSentHandler;
        }

        /// <summary>
        /// Creates block from copy of given data.
        /// </summary>
        /// <param name="data">Data that will be copied into the created block.</param>
        /// <returns>The created block.</returns>
        public Block CreateConstantBlock(byte[] data)
        {
            var memory = getMemoryManager();
            return memory.CreateConstantBlock(data);
        }

        #region Private utilities

        /// <summary>
        /// Gets or creates memory manager if possible. Ensure that configuration won't be changed further.
        /// </summary>
        /// <returns></returns>
        private MemoryManager getMemoryManager()
        {
            if (_memory == null)
            {
                Configuration.Freeze();
                _memory = new MemoryManager(Configuration);
            }

            return _memory;
        }

        #endregion

        #region Firing routines

        /// <summary>
        /// Fires event that handles client accepted on given socket.
        /// </summary>
        /// <param name="socket"></param>
        internal void Fire_AcceptClient(Socket socket)
        {
            var time = DateTime.Now;
            var evt = _clientAcceptedEvents.GetEvent();
            evt.Socket = socket;
            evt.ArrivalTime = time;
            _iochannel.EnqueueEvent(evt);
        }

        internal void Fire_RegisteredClient(Client client)
        {
            _clientAcceptedHandler(client);
        }

        #endregion
    }
}
