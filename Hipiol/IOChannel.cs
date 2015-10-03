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
        /// <summary>
        /// Pool which uses current <see cref="IOProcessor"/>
        /// </summary>
        private readonly IOPool _pool;

        /// <summary>
        /// Controller for receiving events.
        /// </summary>
        private readonly DataReceivedController _receiveController;

        /// <summary>
        /// Controler for sent events.
        /// </summary>
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
            var clientInternal = _pool.Network.RegisterClient(e.Socket, e.ArrivalTime);

            _receiveController.SetClient(clientInternal);
            _pool.Network.Handle_RegisteredClient(_receiveController);
            _receiveController.SetClient(null);
        }

        /// <inheritdoc/>
        internal override void Visit(DataReceivedEvent e)
        {
            _receiveController.SetClient(e.ClientInternal);
            _pool.Network.Handle_DataReceive(_receiveController);
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
            _sentController.SetClient(e.ClientInternal);
            _pool.Network.Handle_DataSent(_sentController);
            _sentController.SetClient(null);
        }
    }
}
