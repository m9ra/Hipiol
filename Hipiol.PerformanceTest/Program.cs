using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.Net;
using System.Net.Sockets;

using Hipiol.PerformanceTest.Stats;
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
            var clientCount = 10000;
            var iterationCount = 20000;

            var expectedSendCount = clientCount * iterationCount;


            var utils = new TestUtils();
            utils.StartServer(clientCount);
            var data = utils.GetRandomData(2048);

            Console.WriteLine("Connecting clients");
            var clients = utils.GetIdentifiedConnectedClients(clientCount);

            var rnd = new Random(1);

            Console.WriteLine("Starting test");
            var startTime = DateTime.Now;

            for (var testIndex = 0; testIndex < iterationCount; ++testIndex)
            {
                var clientIndex = rnd.Next(clients.Length);
                var client = clients[clientIndex];
                client.SendData(data);
            }

            Console.WriteLine(" waiting");
            while (utils.PendingBytes > 0)
            {
                Thread.Sleep(1);
            }

            var endTime = DateTime.Now;
            Console.WriteLine("End\n");


            var connectionTimes = utils.GetConnectionTimes();
            var transferTimes = utils.GetTransferTimes();

            PrintPercentage("Transfer times",transferTimes);
            PrintPercentage("Connection times", connectionTimes);

            var duration = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine("Test duration {0:0.000}", duration);
        }

        private static void PrintPercentage(string caption, IEnumerable<double> times)
        {
            Console.WriteLine(caption);
            var percentage = new Percentage(times, Percentage.StandardPercentage);
            for (var i = 0; i < percentage.PercentCount; ++i)
            {
                var time = percentage.GetThreshold(i);
                var count = percentage.GetCount(i);
                var percent = percentage.GetPercent(i);
                Console.WriteLine("\t{0,3}% {1,5}\t{2:0.000}ms", percent * 100, count, time);
            }
        }
    }
}
