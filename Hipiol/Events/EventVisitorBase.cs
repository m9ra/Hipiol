using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Network;

namespace Hipiol.Events
{
    abstract class EventVisitorBase
    {
        abstract internal void Visit(ClientAcceptedEvent e);

        abstract internal void Visit(DataReceivedEvent e);

        abstract internal void Visit(DataSendEvent e);

        abstract internal void Visit(DataSentEvent e);
    }
}
