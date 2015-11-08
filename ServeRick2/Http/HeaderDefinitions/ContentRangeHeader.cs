using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using ServeRick2.Http.Parsing;

namespace ServeRick2.Http.HeaderDefinitions
{
    class ContentRangeHeader : Header<Range>
    {
        /// <summary>
        /// Field where 'from number' of the range is stored.
        /// </summary>
        FieldInfo _fromStorage;

        /// <summary>
        /// Field where 'to number' of the range is stored.
        /// </summary>
        FieldInfo _toStorage;

        ContentRangeHeader()
            : base("Content-Range")
        {
        }

        /// <inheritdoc/>
        internal override Expression buildBodyParser(AutomatonBuilderContext context)
        {
            throw new NotImplementedException();
        }
    }
}
