using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

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
            var iterationCount = 100;

            var expectedSendCount = clientCount * iterationCount;


            var utils = new TestUtils();
            utils.StartServer(clientCount);
            var data = utils.GetRandomData(1024);

            var clients = utils.GetIdentifiedConnectedClients(clientCount);

            var rnd = new Random(1);

            var startTime = DateTime.Now;
            var sendInfos = new SendInfo[iterationCount];
            for (var testIndex = 0; testIndex < iterationCount; ++testIndex)
            {
                var clientIndex = rnd.Next(clients.Length);
                var client = clients[clientIndex];
                var sendInf = client.SendData(data);
                sendInfos[testIndex] = sendInf;
            }

            while (utils.PendingBytes > 0)
            {
                Thread.Sleep(1);
            }
            var endTime = DateTime.Now;

            var times = utils.GetTransferTimes();
            foreach (var time in times)
            {
                Console.WriteLine(time + "ms");
            }

            var duration = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine("Test duration {0:0.000}", duration);
        }
    }
}
