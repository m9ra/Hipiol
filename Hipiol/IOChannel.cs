using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal IOProcessor(IOPool pool)
        {
            _pool = pool;
        }

        /// <inheritdoc/>
        internal override void Visit(ClientAcceptedEvent e)
        {
            var client = _pool.Network.RegisterClient(e.Socket, e.ArrivalTime);
            _pool.Fire_RegisteredClient(client);
        }
    }
}
