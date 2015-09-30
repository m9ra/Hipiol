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
            //var ioDelayReceive_95 = TestReceiveDelay_95();
            var ioDelaySend_95 = TestSendDelay_95();
            var avgReceiveSpeed = TestAvgReceiveSpeed();
        }

        #region Benchmark implementations

        static double TestReceiveDelay_95()
        {
            var clientCount = 10000;
            var iterationCount = 20000;

            var utils = new TestUtils();
            utils.StartServer(TestControllers.Receive, clientCount);
            var data = utils.GetRandomData(1024);

            Console.WriteLine("Connecting clients");
            var clients = utils.GetIdentifiedConnectedClients(clientCount);

            var rnd = new Random(1);

            Console.WriteLine("Starting test");
            var startTime = DateTime.Now;

            //send small chunks to random clients
            //(can discover troubles in bad handling of large amounts of clients)
            for (var testIndex = 0; testIndex < iterationCount; ++testIndex)
            {
                var clientIndex = rnd.Next(clients.Length);
                var client = clients[clientIndex];
                client.SendData(data);
            }

            Console.WriteLine(" waiting");
            utils.WaitOnPendingData();
            var endTime = DateTime.Now;
            Console.WriteLine("End\n");


            var connectionTimes = utils.GetConnectionTimes();
            var transferTimes = utils.GetTransferTimes();

            var transferPercentage = PrintPercentage("Transfer times", transferTimes);
            PrintPercentage("Connection times", connectionTimes);

            var duration = (endTime - startTime).TotalMilliseconds;

            Console.WriteLine("Test duration {0:0.000}", duration);

            return transferPercentage.GetThreshold(0.95);
        }

        static double TestSendDelay_95()
        {
            var iterationCount = 20000;

            var utils = new TestUtils();
            utils.StartServer(TestControllers.SendRandom);


            Console.WriteLine("Starting test");
            var startTime = DateTime.Now;

            var buffer = new byte[1024];
            for (var testIndex = 0; testIndex < iterationCount; ++testIndex)
            {
                var client = utils.GetClient();
                client.ConnectWithIdentification();
                var receivedBytes = client.Receive(buffer);
                break;
            }

            Console.WriteLine(" waiting");
            Thread.Sleep(1000);
            utils.WaitOnPendingData();
            var endTime = DateTime.Now;
            Console.WriteLine("End\n");

            throw new NotImplementedException();
        }


        static double TestSendDelay()
        {
            throw new NotImplementedException();
        }

        static double TestAvgReceiveSpeed()
        {
            throw new NotImplementedException();
        }
        #endregion

        private static Percentage PrintPercentage(string caption, IEnumerable<double> times)
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

            return percentage;
        }
    }
}
