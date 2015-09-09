using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Hipiol.PerformanceTest.Network
{
    class TestClient
    {
        private readonly TestServer _server;

        private readonly TcpClient _tcpClient;

        internal TestClient(TestServer server)
        {
            _server = server;
            _tcpClient = new TcpClient();
        }

        internal void Connect()
        {
            _tcpClient.Connect(IPAddress.Loopback, _server.ServerPort);
        }

        internal SendInfo SendData(SendableData data)
        {
            throw new NotImplementedException();
        }
    }
}
