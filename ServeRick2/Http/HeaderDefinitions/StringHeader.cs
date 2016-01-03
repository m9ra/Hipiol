using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using ServeRick2.Http.Parsing;

namespace ServeRick2.Http.HeaderDefinitions
{
    abstract class StringHeader : Header<string>
    {
        private int _blobIndex = -1;

        internal StringHeader(string name)
            : base(name)
        {
        }

        internal override Expression buildBodyParser(AutomatonBuilderContext context)
        {
            if(_blobIndex>0)
                throw new NotSupportedException("Cannot build header twice");

            _blobIndex = context.RegisterBlobReader();
            return context.SharedBlobLineReader(_blobIndex);
        }
    }
}
