using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.PerformanceTest.Network;

namespace Hipiol.PerformanceTest
{
    class TestUtils
    {
        private TestServer _server;

        internal TestServer StartServer(int maxParallelClientCount)
        {
            if (_server != null)
                throw new NotSupportedException("Cannot start server twice");

            _server = new TestServer(maxParallelClientCount);

            return _server;
        }

        internal SendableData GetRandomData()
        {
            return new SendableData();
        }

        internal TestClient[] GetConnectedClients(int clientCount)
        {
            var clients = GetClients(clientCount);
            for (var i = 0; i < clients.Length; ++i)
            {
                clients[i].Connect();
            }

            return clients;
        }

        internal TestClient[] GetClients(int clientCount)
        {
            if (_server == null)
                throw new NotSupportedException("Cannot get clients, when server is not running");

            var clients = new List<TestClient>();
            for (var i = 0; i < clientCount; ++i)
            {
                clients.Add(new TestClient(_server));
            }

            return clients.ToArray();
        }
    }
}
