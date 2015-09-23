using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Events;
using Hipiol.Memory;

namespace Hipiol.Network
{
    class DataSendEvent : EventBase
    {
        /// <summary>
        /// Client which is sending data.
        /// </summary>
        internal Client Client;

        /// <summary>
        /// Block with data to sent.
        /// </summary>
        internal Block Block;

        /// <inheritdoc/>
        protected override void recycle()
        {
            Block = null;
        }

        /// <inheritdoc/>
        internal override void Accept(EventVisitorBase visitor)
        {
            throw new NotImplementedException();
        }
    }
}
