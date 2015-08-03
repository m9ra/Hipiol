using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets handlers for client acceptance and disconnections.
        /// </summary>
        /// <param name="clientAcceptedHandler">Accepted clients are reported by this handler.</param>
        /// <param name="clientDisconnectedHandler">Client disconnection is reported by this handler.</param>
        public void SetClientHandlers(ClientAccepted clientAcceptedHandler, ClientDisconnected clientDisconnectedHandler)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set handlers for network data manipulation.
        /// </summary>
        /// <param name="dataReceivedHandler">Received data are reported by this handler.</param>
        /// <param name="dataBlockSentHandler">Successful completition of data block sending is reporeted by this handler. It is called just once for every <see cref="Send"/> if sending is successful.</param>
        public void SetDataHandlers(DataReceived dataReceivedHandler, DataBlockSent dataBlockSentHandler)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates block from copy of given data.
        /// </summary>
        /// <param name="data">Data that will be copied into the created block.</param>
        /// <returns>The created block.</returns>
        public Block CreateConstantBlock(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
