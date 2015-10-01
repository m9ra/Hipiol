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

        internal IOPool IOPool { get; private set; }

        internal void InitializePool(IOPool pool)
        {
            if (IOPool != null)
                throw new NotSupportedException("Cannot reset IOPool");

            if (pool == null)
                throw new ArgumentNullException("pool");

            IOPool = pool;
            OnPoolInitialization();
        }

        internal virtual void OnPoolInitialization()
        {
            //by default we do nothing
        }

        internal virtual void DataReceived(DataReceivedController controller, Block block)
        {
            //by default we do nothing
        }

        internal virtual void DataSent(DataSentController controller)
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
