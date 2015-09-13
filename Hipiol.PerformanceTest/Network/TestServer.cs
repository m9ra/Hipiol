using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol;
using Hipiol.Memory;
using Hipiol.Network;

namespace Hipiol.PerformanceTest.Network
{
    class TestServer
    {
        internal static readonly int MaxClientCount = 2000;

        internal readonly int ServerPort = 12345;

        private readonly IOPool _pool;

        private readonly ServerClient[] _clients = new ServerClient[MaxClientCount];

        private volatile int _nextClient = 0;

        internal volatile int ReceivedBytesCount;

        internal IEnumerable<ServerClient> Clients { get { return _clients.Take(_nextClient); } }

        internal int ClientCount { get { return _nextClient; } }

        internal TestServer(int maxParallelClientCount)
        {
            _pool = new IOPool();

            _pool.SetClientHandlers(_clientAccepted, _clientDisconnected);
            _pool.SetDataHandlers(_dataReceived, _dataBlockSent);
            _pool.StartListening(ServerPort);

            for (var i = 0; i < _clients.Length; ++i)
            {
                _clients[i] = new ServerClient();
            }
        }

        private IEnumerable<Block> prepareData(IOPool pool)
        {
            var blocks = new List<Block>();
            for (var i = 0; i < 100; ++i)
            {
                var block = pool.CreateConstantBlock(new byte[1000]);
                blocks.Add(block);
            }

            return blocks;
        }

        private void _dataReceived(DataTransferController controller, Block block)
        {
            if (block == null)
            {
                //no data are available for the client - there is probably an timeout
                //we won't generate any response
                controller.Disconnect();
                return;
            }

            if (controller.ReceivedBytes == 0)
                //nothing to do
                return;

            ReceivedBytesCount += controller.ReceivedBytes;

            var client = controller.Client.Tag as ServerClient;
            client.ReportData(controller.ReceivedBytes);
        }

        private void _dataBlockSent(DataTransferController controller)
        {
            throw new NotImplementedException();
        }

        private void _clientAccepted(ClientController controller)
        {
            var serverClient = _clients[_nextClient];
            serverClient.ReportAccept();

            var client = controller.Client;
            controller.SetTag(serverClient);
            _nextClient += 1;

            //we will allow receiving of data for the client.
            controller.AllowReceive(0);
        }

        private void _clientDisconnected(ClientController controller)
        {
            var serverClient = controller.Client.Tag as ServerClient;
            serverClient.ReportDisconnection();
        }

        internal ServerClient GetClient(int clientIndex)
        {
            return _clients[clientIndex];
        }
    }
}
