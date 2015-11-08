using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection;

using ServeRick2.Http.Parsing;

namespace ServeRick2.Http.HeaderDefinitions
{
    abstract class IntHeader : Header<int>
    {
        internal readonly string FieldName;

        internal IntHeader(string name, string fieldName)
            : base(name)
        {
            FieldName = fieldName;
        }

        /// <inheritdoc/>
        internal override Expression buildBodyParser(AutomatonBuilderContext context)
        {
            return context.ReadInt(FieldName);
        }
    }
}
