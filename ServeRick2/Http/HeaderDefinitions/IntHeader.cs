using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection;

namespace ServeRick2.Http.HeaderDefinitions
{
    abstract class IntHeader : Header<int>
    {
        /// <summary>
        /// Field in request, where parsed number is stored.
        /// </summary>
        private readonly FieldInfo _intStorage;

        internal IntHeader(string name)
            : base(name)
        {
        }

        /// <inheritdoc/>
        protected override void buildParser()
        {
            ReadInt(_intStorage);
        }
    }
}
