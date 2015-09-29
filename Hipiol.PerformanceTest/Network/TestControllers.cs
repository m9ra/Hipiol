using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.PerformanceTest.Network.ServerControllers;

namespace Hipiol.PerformanceTest.Network
{
    static class TestControllers
    {
        internal static readonly ServerControllerBase Receive = new ReceiveController();


    }
}
