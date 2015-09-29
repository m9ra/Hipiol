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
        internal override void DataReceived(DataTransferController controller, Block block)
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
