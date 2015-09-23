using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.Network
{
    /// <summary>
    /// Representation of network client.
    /// </summary>
    public struct Client
    {


        /// <summary>
        /// Key which ensures that clients data are still present at given <see cref="Index"/>.
        /// </summary>
        internal readonly int Key;

        /// <summary>
        /// Index in <see cref="NetworkManager"/>.
        /// </summary>
        internal readonly int Index;

        internal Client(int index, int key)
        {
            Index = index;
            Key = key;
        }
    }
}
