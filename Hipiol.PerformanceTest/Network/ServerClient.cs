using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.PerformanceTest.Network
{
    class ServerClient
    {
        internal DateTime AcceptTime;

        internal DateTime DisconnectionTime;

        internal readonly DateTime[] DataArrivals = new DateTime[100];

        internal readonly int[] ByteAmounts = new int[100];

        private int _arrivalsCount = 0;

        private int _bytesReceived = 0;

        internal void ReportData(int bytes)
        {
            _bytesReceived += bytes;

            DataArrivals[_arrivalsCount] = DateTime.Now;
            ByteAmounts[_arrivalsCount] = _bytesReceived;

            _arrivalsCount += 1;
        }

        internal void ReportDisconnection()
        {
            DisconnectionTime = DateTime.Now;
        }

        internal void ReportAccept()
        {
            AcceptTime = DateTime.Now;
        }

        internal DateTime GetReceiveTime(int bytes)
        {
            for (var i = 0; i < _arrivalsCount; ++i)
            {
                if (ByteAmounts[i] >= _bytesReceived)
                    return DataArrivals[i];
            }

            return DateTime.MaxValue;
        }
    }
}
