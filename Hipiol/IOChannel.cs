﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

using Hipiol.Events;
using Hipiol.Memory;
using Hipiol.Network;

namespace Hipiol
{
    class IOChannel : EventChannelBase<EventBase>
    {
        /// <summary>
        /// Processor that executes events code.
        /// </summary>
        internal readonly IOProcessor Processor;

        internal IOChannel(IOPool pool)
        {
            Processor = new IOProcessor(pool);
        }

        protected override void Process(EventBase eventObject)
        {
            eventObject.Accept(Processor);
            eventObject.Recycle();
        }
    }

    class IOProcessor : EventVisitorBase
    {
        private readonly IOPool _pool;

        private readonly DataTransferController _controller;

        internal IOProcessor(IOPool pool)
        {
            _pool = pool;
            _controller = new DataTransferController(pool);
        }

        /// <inheritdoc/>
        internal override void Visit(ClientAcceptedEvent e)
        {
            var client = _pool.Network.RegisterClient(e.Socket, e.ArrivalTime);
            _controller.SetClient(client);
            _pool.Handle_RegisteredClient(_controller);
            _controller.SetClient(null);
        }

        /// <inheritdoc/>
        internal override void Visit(DataReceivedEvent e)
        {
            var client = e.ClientInternal;

            //is called when client received data.
            if (client.ReceiveEventArgs.SocketError != SocketError.Success)
                throw new NotImplementedException("Handle socket errors");

            _controller.SetClient(client);
            _pool.Handle_DataReceive(_controller);
            if (client.AllowReceiving)
                _pool.Network.RequestReceiving(client);

            _controller.SetClient(null);
        }

        /// <inheritdoc/>
        internal override void Visit(DataSendEvent e)
        {
            var clientInternal = _pool.Network.GetClientInternal(e.Client);
            if (clientInternal == null)
                //client is no more available
                return;

            //we just forward send handling to NetworkManager
            _pool.Network.Send(clientInternal, e.Block, e.DataOffset, e.DataSize);
        }

        /// <inheritdoc/>
        internal override void Visit(DataSentEvent e)
        {
            var clientInternal = e.ClientInternal;
            _pool.Network.Handle_DataSent(clientInternal);
        }
    }
}
