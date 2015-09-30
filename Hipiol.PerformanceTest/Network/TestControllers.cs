using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hipiol.Memory;
using Hipiol.PerformanceTest.Network.ServerControllers;

namespace Hipiol.PerformanceTest.Network
{
    delegate IEnumerable<Block> BlocksInitializer(IOPool pool);

    static class TestControllers
    {
        private static readonly BlocksInitializer RandomBlocksFactory = (p) => createRandomBlocks(1024, 10, p);

        #region Controllers

        internal static readonly ServerControllerBase Receive = new ReceiveController();

        internal static readonly ServerControllerBase SendRandom = new SendController(RandomBlocksFactory);

        #endregion

        internal static IEnumerable<Block> createRandomBlocks(int blockSize, int blockCount, IOPool pool)
        {
            var result = new List<Block>();
            for (var i = 0; i < blockCount; ++i)
            {
                var data = new byte[blockSize];
                var block = pool.CreateConstantBlock(data);
                result.Add(block);
            }

            return result;
        }
    }
}
