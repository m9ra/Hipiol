using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http.HeaderDefinitions
{
    abstract class StringHeader : Header<string>
    {
        internal StringHeader(string name)
            : base(name)
        {
        }
    }
}
