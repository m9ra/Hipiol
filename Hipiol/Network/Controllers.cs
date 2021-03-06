﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;

namespace Hipiol.Network
{
    /// <summary>
    /// Controlls settings of client. Is available only in <see cref="IOPool"/> callbacks.
    /// </summary>
    public class ClientController
    {
        /// <summary>
        /// Client that is controlled by current controller.
        /// </summary>
        internal ClientInternal ClientInternal;

        /// <summary>
        /// Pool that is related to current controller.
        /// </summary>
        internal protected readonly IOPool Pool;

        /// <summary>
        /// Handle to the controlled client.
        /// </summary>
        public Client Client { get { return ClientInternal.Client; } }

        /// <summary>
        /// Tag assigned to controlled client.
        /// </summary>
        public object ClientTag { get { return ClientInternal.Tag; } }

        internal ClientController(IOPool pool)
        {
            Pool = pool;
        }

        /// <summary>
        /// Enables receiving for the controlled client.
        /// <remarks>Data receiving is not enabled for accepted clients by default.</remarks>
        /// </summary>
        /// <param name="timeout">Timeout, which is in milliseconds, will fire data received event with an <c>null</c> block after expiration. Zero timeout will wait for infinity. Negative timeout is an error.</param>
        public void AllowReceive(int timeout = 0)
        {
            Pool.Network.StartReceiving(ClientInternal, timeout, Pool.Memory.GetIOBlock());
        }

        /// <summary>
        /// Sends block to the controlled client.
        /// </summary>
        /// <param name="block">Block to send.</param>
        public void Send(Block block)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disconnects controlled client from the current pool.
        /// </summary>
        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets client that is controlled by current controller.
        /// </summary>
        /// <param name="client">The client.</param>
        internal void SetClient(ClientInternal client)
        {
            ClientInternal = client;
        }

        /// <summary>
        /// Set given tag to controlled client.
        /// </summary>
        /// <param name="tag">The tag that is set.</param>
        public void SetTag(object tag)
        {
            ClientInternal.Tag = tag;
        }
    }

    /// <summary>
    /// Controlls settings of <see cref="ClientController"/>, and routines for data receiving.
    /// </summary>
    public class DataReceivedController : ClientController
    {
        /// <summary>
        /// Block which contains received data.
        /// </summary>
        internal Block ReceivedBlock { get { return ClientInternal.ReceiveBuffer; } }

        /// <summary>
        /// How many bytes has been received.
        /// </summary>
        public int ReceivedBytes { get { return ClientInternal.ReceiveEventArgs.BytesTransferred; } }

        internal DataReceivedController(IOPool pool)
            : base(pool)
        {
        }
    }

    /// <summary>
    /// Controlls settings of <see cref="ClientController"/> after data sent
    /// </summary>
    public class DataSentController : ClientController
    {
        /// <summary>
        /// Block which has been sent.
        /// </summary>
        internal Block SentBlock { get { return ClientInternal.ActualSendBlock.Block; } }

        /// <summary>
        /// How many bytes has been sent.
        /// </summary>
        public int SentBytes { get { return ClientInternal.SendEventArgs.BytesTransferred; } }

        internal DataSentController(IOPool pool)
            :base(pool)
        {
        }
    }
}
