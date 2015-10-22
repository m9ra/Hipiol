using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http.HeaderDefinitions
{
    class Range
    {
        /// <summary>
        /// Start offset of the range.
        /// </summary>
        internal readonly int From = 0;

        /// <summary>
        /// End offset of the range.
        /// </summary>
        internal readonly int To = 0;
    }
}
