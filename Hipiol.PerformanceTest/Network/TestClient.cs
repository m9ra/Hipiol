using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.Threading;

namespace Hipiol.PerformanceTest.Network
{
    class TestClient
    {
        private readonly TestServer _server;

        private readonly TcpClient _tcpClient;

        private readonly List<DateTime> _sendTimes = new List<DateTime>();

        private readonly List<int> _sentBytes = new List<int>();

        internal int SendCount { get { return _sendTimes.Count; } }

        internal bool IsIdentified { get { return ServerClient != null; } }

        internal int TotalSentBytesCount { get; private set; }

        internal DateTime ConnectionTime { get; private set; }

        internal ServerClient ServerClient { get; private set; }

        internal TestClient(TestServer server)
        {
            _server = server;
            _tcpClient = new TcpClient();
        }

        internal void Connect()
        {
            _tcpClient.NoDelay = true;
            ConnectionTime = DateTime.Now;
            _tcpClient.Connect(IPAddress.Loopback, _server.ServerPort);
        }

        internal void ConnectWithIdentification()
        {
            var originalClientCount = _server.ClientCount;
            Connect();
            while (_server.ClientCount != originalClientCount + 1)
            {
                //Spin lock waiting - we need maximal responsivenes
                //Waiting times are short -> overhead is not significant
            }

            ServerClient = _server.GetClient(originalClientCount);
        }

        internal SendInfo SendData(SendableData data)
        {
            TotalSentBytesCount += data.Data.Length;

            _sendTimes.Add(DateTime.Now);
            _sentBytes.Add(TotalSentBytesCount);
            var result = _tcpClient.Client.Send(data.Data);
            if (result != data.Data.Length)
                throw new NotImplementedException("partial sending");

            return new SendInfo();
        }

        internal DateTime GetSendTime(int i)
        {
            return _sendTimes[i];
        }

        internal int SentBytesCount(int i)
        {
            return _sentBytes[i];
        }

        internal int Receive(byte[] buffer)
        {
            return _tcpClient.Client.Receive(buffer);
        }
    }
}
