﻿using System;
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

        /// <summary>
        /// Offset of data in block to send.
        /// </summary>
        internal int DataOffset;

        /// <summary>
        /// Size of data in block to send.
        /// </summary>
        internal int DataSize;

        /// <inheritdoc/>
        protected override void recycle()
        {
            Block = null;
            DataOffset = 0;
            DataSize = 0;
        }

        /// <inheritdoc/>
        internal override void Accept(EventVisitorBase visitor)
        {
            visitor.Visit(this);
        }
    }
}
