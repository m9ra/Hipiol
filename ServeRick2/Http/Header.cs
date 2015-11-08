using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using ServeRick2.Http.Parsing;

namespace ServeRick2.Http
{
    /// <summary>
    /// Generator for action expression.
    /// </summary>
    /// <param name="request">Expression with stored request.</param>
    /// <param name="currentByte">Byte which caused action execution.</param>
    internal delegate Expression ActionCreator(Expression request, Expression currentByte);

    public abstract class Header
    {
        /// <summary>
        /// Name of the header.
        /// </summary>
        internal readonly string Name;

        /// <summary>
        /// Template method which builds parser for the header.
        /// </summary>
        internal abstract Expression buildBodyParser(AutomatonBuilderContext context);

        protected Header(string name)
        {
            Name = name;
        }

        internal Expression BuildParser(AutomatonBuilderContext context)
        {
            return buildBodyParser(context);
        }
    }

    public abstract class Header<T> : Header
    {
        internal Header(string name)
            : base(name)
        {
        }
    }
}
