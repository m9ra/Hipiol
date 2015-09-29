using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using Hipiol.PerformanceTest.Network;

namespace Hipiol.PerformanceTest
{
    class TestUtils
    {
        private TestServer _server;

        private readonly List<TestClient> _testClients = new List<TestClient>();

        internal int PendingBytes { get { return SentBytesCount - _server.ReceivedBytesCount; } }

        internal int SentBytesCount { get { return _testClients.Sum(c => c.TotalSentBytesCount); } }

        internal TestServer StartServer(int maxParallelClientCount, ServerControllerBase serverController)
        {
            if (_server != null)
                throw new NotSupportedException("Cannot start server twice");

            _server = new TestServer(maxParallelClientCount, serverController);

            return _server;
        }

        internal SendableData GetRandomData(int size)
        {
            var randomData = new byte[size];
            return new SendableData(randomData);
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

        internal TestClient[] GetIdentifiedConnectedClients(int clientCount)
        {
            var clients = GetClients(clientCount);
            for (var i = 0; i < clients.Length; ++i)
            {
                clients[i].ConenctWithIdentification();
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
                var client = new TestClient(_server);
                _testClients.Add(client);
                clients.Add(client);
            }

            return clients.ToArray();
        }

        internal IEnumerable<double> GetTransferTimes()
        {
            var transferTimes = new List<double>();
            foreach (var client in _testClients)
            {
                if (!client.IsIdentified)
                    throw new NotSupportedException("Cannot get transfer time for non-identified client.");

                for (var i = 0; i < client.SendCount; ++i)
                {
                    var sendTime = client.GetSendTime(i);
                    var sentBytes = client.SentBytesCount(i);

                    var receiveTime = client.ServerClient.GetReceiveTime(sentBytes);

                    var transferTime = receiveTime - sendTime;
                    transferTimes.Add(transferTime.TotalMilliseconds);
                }
            }

            return transferTimes;
        }

        internal void WaitOnPendingData()
        {
            while (PendingBytes > 0)
            {
                Thread.Sleep(1);
            }
        }

        internal IEnumerable<double> GetConnectionTimes()
        {
            var connectionTimes = new List<double>();
            foreach (var client in _testClients)
            {
                if (!client.IsIdentified)
                    throw new NotSupportedException("Cannot get transfer time for non-identified client.");

                var connectionTime = client.ServerClient.AcceptTime - client.ConnectionTime;
                connectionTimes.Add(connectionTime.TotalMilliseconds);
            }

            return connectionTimes;
        }
    }
}
