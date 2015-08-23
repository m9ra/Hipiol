using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

using Hipiol.Events;

namespace Hipiol.Network
{
    class ClientAcceptedEvent : EventBase
    {
        /// <summary>
        /// Socket of accepted client.
        /// </summary>
        internal Socket Socket;

        /// <summary>
        /// Time when client has been accepted.
        /// </summary>
        internal  DateTime ArrivalTime;

        /// <inheritdoc/>
        protected override void recycle()
        {
            Socket = null;
            ArrivalTime = DateTime.MinValue;
        }

        /// <inheritdoc/>
        internal override void Accept(EventVisitorBase visitor)
        {
            visitor.Visit(this);
        }
    }
}
