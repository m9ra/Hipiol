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
    class SimpleServer
    {
        internal readonly int ServerPort = 12345;

        private readonly IOPool _pool;

        private readonly HashSet<Client> _clients = new HashSet<Client>();

        private readonly List<Block> _data;

        private readonly Dictionary<Client, int> _clientState = new Dictionary<Client, int>();

        internal SimpleServer()
        {
            _pool = new IOPool();
            _data = prepareData(_pool).ToList();

            _pool.SetClientHandlers(_clientAccepted, _clientDisconnected);
            _pool.SetDataHandlers(_dataReceived, _dataBlockSent);
            _pool.StartListening(ServerPort);
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

        private void _dataReceived(Client client, Block block)
        {
            if (block == null)
            {
                //no data are available for the client - there is probably an timeout
                //we won't generate any response
                _pool.Disconnect(client);
                return;
            }

            throw new NotImplementedException();
        }

        private void _dataBlockSent(Client client)
        {
            var state = _clientState[client];
            _pool.Send(client, _data[state]);

            var newstate = state + 1;
            if (newstate >= _data.Count)
                _pool.Disconnect(client);

            _clientState[client] = newstate;
        }

        private void _clientAccepted(Client client)
        {
            _clients.Add(client);
            _clientState.Add(client, 0);

            //we will allow receiving of data for the client.
            _pool.AllowReceive(client, 1000);
        }

        private void _clientDisconnected(Client client)
        {
            _clients.Remove(client);
            _clientState.Remove(client);
        }
    }
}
