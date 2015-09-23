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
    /// <param name="controller">Controller with accepted client.</param>
    public delegate void ClientAccepted(ClientController controller);

    /// <summary>
    /// Delegate used for client disconnected events.
    /// <remarks>Disconnected event is raised just once for every accepted client.</remarks>
    /// </summary>
    /// <param name="controller">Controller with disconnected client.</param>
    public delegate void ClientDisconnected(ClientController controller);

    /// <summary>
    /// Delegate used for data received events.
    /// </summary>
    /// <param name="controller">Client which received the data.</param>
    /// <param name="block">Block where received data are stored.</param>
    public delegate void DataReceived(DataTransferController controller, Block block);

    /// <summary>
    /// Delegate used for data sent completition events.
    /// </summary>
    /// <param name="client">Client which data block sending was completed.</param>
    public delegate void DataBlockSent(DataTransferController controller);

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

        /// <summary>
        /// Storage for data received events.
        /// </summary>
        private readonly EventStorage<DataReceivedEvent> _dataReceivedEvents = new EventStorage<DataReceivedEvent>();
        
        /// <summary>
        /// Storage for data send events.
        /// </summary>
        private readonly EventStorage<DataSendEvent> _dataSendEvents = new EventStorage<DataSendEvent>();

        #endregion

        /// <summary>
        /// Manager thata handleas network communication.
        /// </summary>
        internal NetworkManager Network { get; private set; }

        /// <summary>
        /// Manager that handles memory blocks.
        /// </summary>
        internal MemoryManager Memory { get { return getMemoryManager(); } }

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

        /// <summary>
        /// Handler for accepted clients
        /// </summary>
        private ClientAccepted _clientAcceptedHandler;

        /// <summary>
        /// Handler for disconnected clients
        /// </summary>
        private ClientDisconnected _clientDisconnectedHandler;

        /// <summary>
        /// Handler for received data.
        /// </summary>
        private DataReceived _dataReceivedHandler;

        /// <summary>
        /// Handler called after successful completition of data block sending is reporeted by this handler.
        /// </summary>
        private DataBlockSent _dataBlockSentHandler;

        public IOPool()
        {
            //set default configuration
            Configuration = new PoolConfiguration();

            _iochannel = new IOChannel(this);

            _eventThread = new Thread(_iochannel.ProcessEvents);
            _eventThread.Start();
        }

        /// <summary>
        /// Sends given block to given client.
        /// </summary>
        /// <param name="client">Client whom the data will be sent.</param>
        /// <param name="block">Block that will be sent.</param>
        public void Send(Client client, Block block)
        {
            Fire_Send(client, block);
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
            if (socket == null)
                throw new ArgumentNullException("socket");

            var time = DateTime.Now;
            var evt = _clientAcceptedEvents.GetEvent();

            evt.Socket = socket;
            evt.ArrivalTime = time;
            _iochannel.EnqueueEvent(evt);
        }

        /// <summary>
        /// Fires event that handles client receiving.
        /// </summary>
        /// <param name="clientInternal">Client which receiving is handled.</param>
        internal void Fire_DataReceive(ClientInternal clientInternal)
        {
            var evt = _dataReceivedEvents.GetEvent();
            evt.ClientInternal = clientInternal;
            _iochannel.EnqueueEvent(evt);
        }

        /// <summary>
        /// Fires event that handles client sending.
        /// </summary>
        /// <param name="client">Client whom the data will be sent.</param>
        /// <param name="block">The data to send.</param>
        private void Fire_Send(Client client, Block block)
        {
            var evt = _dataSendEvents.GetEvent();
            evt.Client = client;
            evt.Block = block;
            _iochannel.EnqueueEvent(evt);
        }
        #endregion

        #region Event handlers

        /// <summary>
        /// Handle client that has been registered.
        /// </summary>
        /// <param name="controller">Controller of registered client.</param>
        internal void Handle_RegisteredClient(ClientController controller)
        {
            _clientAcceptedHandler(controller);
        }

        /// <summary>
        /// Handle client that has received data.
        /// </summary>
        /// <param name="controller">Controller of the client.</param>
        internal void Handle_DataReceive(DataTransferController controller)
        {
            _dataReceivedHandler(controller, controller.ReceivedBlock);
        }

        #endregion
    }
}
