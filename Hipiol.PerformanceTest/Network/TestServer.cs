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
        internal static readonly int MaxClientCount = 10000;

        internal readonly int ServerPort = 12345;

        private readonly ServerControllerBase _serverController;

        private readonly IOPool _pool;

        private readonly ServerClient[] _clients = new ServerClient[MaxClientCount];

        private volatile int _nextClient = 0;

        internal volatile int ReceivedBytesCount;

        internal volatile int SentBytesCount;

        internal IEnumerable<ServerClient> Clients { get { return _clients.Take(_nextClient); } }

        internal int ClientCount { get { return _nextClient; } }

        internal TestServer(int maxParallelClientCount, ServerControllerBase serverController)
        {
            _pool = new IOPool();
            _serverController = serverController;
            _serverController.InitializePool(_pool);

            _pool.SetClientHandlers(_clientAccepted, _clientDisconnected);
            _pool.SetDataHandlers(_dataReceived, _dataBlockSent);
            _pool.StartListening(ServerPort);

            for (var i = 0; i < _clients.Length; ++i)
            {
                _clients[i] = new ServerClient();
            }
        }

        private void _dataReceived(DataReceivedController controller, Block block)
        {
            ReceivedBytesCount += controller.ReceivedBytes;

            _serverController.DataReceived(controller, block);
        }

        private void _dataBlockSent(DataSentController controller)
        {
            SentBytesCount += controller.SentBytes;

            _serverController.DataSent(controller);
        }

        private void _clientAccepted(ClientController controller)
        {
            var serverClient = _clients[_nextClient];
            serverClient.ReportAccept();

            var client = controller.Client;
            controller.SetTag(serverClient);
            _nextClient += 1;

            _serverController.ClientAccepted(controller);            
        }

        private void _clientDisconnected(ClientController controller)
        {
            var serverClient = controller.ClientTag as ServerClient;
            serverClient.ReportDisconnection();
        }

        internal ServerClient GetClient(int clientIndex)
        {
            return _clients[clientIndex];
        }
    }
}
