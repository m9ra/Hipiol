using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;
using Hipiol.Network;

namespace Hipiol.PerformanceTest.Network.ServerControllers
{
    class SendController : ServerControllerBase
    {
        private Block[] _dataToSend;

        private readonly BlocksInitializer _initializer;

        internal SendController(BlocksInitializer blocksInitializer)
        {
            _initializer = blocksInitializer;
        }

        internal override void OnPoolInitialization()
        {
            _dataToSend = _initializer(IOPool).ToArray();
        }

        internal override void ClientAccepted(ClientController controller)
        {
            var i = 0;
            IOPool.Send(controller.Client, _dataToSend[i], 0, _dataToSend[i].Size);
        }
    }
}
