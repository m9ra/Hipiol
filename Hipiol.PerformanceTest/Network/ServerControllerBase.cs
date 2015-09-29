using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;
using Hipiol.Network;

namespace Hipiol.PerformanceTest.Network
{
    abstract class ServerControllerBase
    {

        internal TestClient TestClient { get; private set; }

        internal virtual void DataReceived(DataTransferController controller, Block block)
        {
            //by default we do nothing
        }

        internal virtual void ClientAccepted(ClientController controller)
        {
            //by default we do nothing
        }

        internal void SetClient(TestClient client)
        {
            TestClient = client;
        }

        internal void ResetClient()
        {
            TestClient = null;
        }
    }
}
