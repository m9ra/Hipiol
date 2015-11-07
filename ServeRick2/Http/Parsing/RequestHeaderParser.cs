using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace ServeRick2.Http.Parsing
{
    internal delegate void ParsingAutomaton(Request request);

    internal delegate Expression AutomatonBuilderDirector(AutomatonBuilderContext context);

    class RequestHeaderParser
    {
        /// <summary>
        /// Headers that will be accepted.
        /// </summary>
        private readonly List<Header> _acceptedHeaders = new List<Header>();

        /// <summary>
        /// Accepts data in the given buffer.
        /// </summary>
        /// <param name="startOffset">Start of data to accept (inclusive).</param>
        /// <param name="endOffset">End of data to accept (exclusive).</param>
        /// <param name="buffer">Buffer with data.</param>
        internal void Accept(int startOffset, int endOffset, byte[] buffer)
        {
            throw new NotImplementedException("Feed the state automaton");
        }

        /// <summary>
        /// Compiles automataon for given headers.
        /// </summary>
        /// <param name="headers">Headers to be compiled.</param>
        /// <returns>The compiled automaton.</returns>
        internal static ParsingAutomaton CompileAutomaton(IEnumerable<Header> headers)
        {
            //first we will parse method
            var builder = new AutomatonBuilder();
            builder.Emit_ActionSwitch(new Dictionary<string, AutomatonBuilderDirector>{
                {"GET",_getMethod},
                {"POST",_postMethod},
                {"PUT",_putMethod}
            });

            //read url
            builder.Emit_ReadString(0, ' ');

            //we skip HTTP version for now
            builder.Emit_PassLine();

            //switch for headers
            var headerSwitch = new Dictionary<string, AutomatonBuilderDirector>();
            foreach (var header in headers)
            {
                headerSwitch.Add(header.Name, header.BuildParser);
            }

            //read headers
            if (headerSwitch.Count > 0)
                builder.Emit_RepeatedActionSwitch(headerSwitch);

            return builder.Compile();
        }

        #region Parsing utilities

        /// <summary>
        /// Creates expression seting method to GET.
        /// </summary>
        /// <param name="context">Build context.</param>
        /// <returns>The created expression.</returns>
        private static Expression _getMethod(AutomatonBuilderContext context)
        {
            return Expression.Assign(context.MethodStorage, Expression.Constant(Method.GET));
        }

        /// <summary>
        /// Creates expression seting method to POST.
        /// </summary>
        /// <param name="context">Build context.</param>
        /// <returns>The created expression.</returns>
        private static Expression _postMethod(AutomatonBuilderContext context)
        {
            return Expression.Assign(context.MethodStorage, Expression.Constant(Method.POST));
        }

        /// <summary>
        /// Creates expression seting method to PUT.
        /// </summary>
        /// <param name="context">Build context.</param>
        /// <returns>The created expression.</returns>
        private static Expression _putMethod(AutomatonBuilderContext context)
        {
            return Expression.Assign(context.MethodStorage, Expression.Constant(Method.PUT));
        }

        #endregion
    }
}
