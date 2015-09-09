using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Events;

namespace Hipiol.Network
{
    class DataReceivedEvent : EventBase
    {
        internal ClientInternal Client;

        protected override void recycle()
        {
            Client = null;
        }

        internal override void Accept(EventVisitorBase visitor)
        {
            visitor.Visit(this);
        }
    }
}
