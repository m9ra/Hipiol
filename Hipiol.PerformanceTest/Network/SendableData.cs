using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;

namespace Hipiol.PerformanceTest.Network
{
    class SendableData
    {
        internal readonly byte[] Data;

        internal SendableData(byte[] data)
        {
            Data = data.ToArray();
        }
    }
}
