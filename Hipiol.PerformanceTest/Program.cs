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
      /*      var server = new TestServer();

            var client = new TcpClient();
            client.Connect(IPAddress.Loopback, server.ServerPort);

            var data = new byte[10000];
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = (byte)i;
            }
            client.Client.Send(data);
            client.Client.Receive(new byte[1000]);
      */
            SkippedTransferTest();
        }

        static void SkippedTransferTest()
        {
            var clientCount = 1000;
            var utils = new TestUtils();
            utils.StartServer(clientCount);
            var data = utils.GetRandomData();

            var clients = utils.GetConnectedClients(clientCount);

            var rnd = new Random(1);

            var iterationCount = 10000;
            var sendInfos=new SendInfo[iterationCount];
            for (var testIndex = 0; testIndex < iterationCount; ++testIndex)
            {
                var clientIndex = rnd.Next(clients.Length);
                var client = clients[clientIndex];
                var sendInf = client.SendData(data);
                sendInfos[testIndex] = sendInf;
            }
        }
    }
}
