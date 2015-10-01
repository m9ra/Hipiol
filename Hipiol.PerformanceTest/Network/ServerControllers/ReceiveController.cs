using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;    
using Hipiol.Network;


namespace Hipiol.PerformanceTest.Network.ServerControllers
{
    class ReceiveController : ServerControllerBase
    {
        internal override void DataReceived(DataReceivedController controller, Block block)
        {
            var client = controller.ClientTag as ServerClient;
            client.ReportData(controller.ReceivedBytes);
        }

        internal override void ClientAccepted(ClientController controller)
        {
            //we will allow receiving of data for the client.
            controller.AllowReceive(0);
        }
    }
}
