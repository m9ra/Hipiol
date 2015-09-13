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
        /// Tag that can be used for attaching information to a client.
        /// When setting the tag - be careful because of struct assignment behaviour.
        /// <remarks>It could be accessed only from pool thread. It is erased after disconnection</remarks>
        /// </summary>
        public object Tag;

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
            Tag = null;
        }
    }
}
