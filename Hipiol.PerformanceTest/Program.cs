using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Hipiol.PerformanceTest.Network;

namespace Hipiol.PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SimpleServer();

            var client = new TcpClient();
            client.Connect(IPAddress.Loopback, server.ServerPort);
            client.Client.Send(new byte[10000]);
            client.Client.Receive(new byte[1000]);
        }
    }
}
