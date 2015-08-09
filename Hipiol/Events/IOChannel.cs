using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.Events
{
    class IOChannel : EventChannelBase<EventBase>
    {
        protected override void Process(EventBase eventObject)
        {
            throw new NotImplementedException();
        }
    }
}
