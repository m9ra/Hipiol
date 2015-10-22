using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http.HeaderDefinitions
{
    class ContentLengthHeader : IntHeader
    {
        internal ContentLengthHeader()
            : base("Content-Length")
        {
        }
    }
}
