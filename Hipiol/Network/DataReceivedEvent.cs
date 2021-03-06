﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Events;

namespace Hipiol.Network
{
    class DataReceivedEvent : EventBase
    {
        /// <summary>
        /// Client which received data.
        /// </summary>
        internal ClientInternal ClientInternal;

        /// <inheritdoc/>
        protected override void recycle()
        {
            ClientInternal = null;
        }

        /// <inheritdoc/>
        internal override void Accept(EventVisitorBase visitor)
        {
            visitor.Visit(this);
        }
    }
}
