using System;
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

        private readonly DataReceivedController _receiveController;

        private readonly DataSentController _sentController;

        internal IOProcessor(IOPool pool)
        {
            _pool = pool;
            _receiveController = new DataReceivedController(pool);
            _sentController = new DataSentController(pool);
        }

        /// <inheritdoc/>
        internal override void Visit(ClientAcceptedEvent e)
        {
            var client = _pool.Network.RegisterClient(e.Socket, e.ArrivalTime);
            _receiveController.SetClient(client);
            _pool.Handle_RegisteredClient(_receiveController);
            _receiveController.SetClient(null);
        }

        /// <inheritdoc/>
        internal override void Visit(DataReceivedEvent e)
        {
            var client = e.ClientInternal;

            //is called when client received data.
            if (client.ReceiveEventArgs.SocketError != SocketError.Success)
                throw new NotImplementedException("Handle socket errors");

            _receiveController.SetClient(client);
            _pool.Handle_DataReceive(_receiveController);
            if (client.AllowReceiving)
                _pool.Network.RequestReceiving(client);

            _receiveController.SetClient(null);
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

            _sentController.SetClient(clientInternal);
            _pool.Handle_DataSent(_sentController);
            _sentController.SetClient(null);

            throw new NotImplementedException("handle sending of chained blocks + ensure that block has been sent completely");
        }
    }
}
