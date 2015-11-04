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
            for (var index = startOffset; index < endOffset; ++startOffset)
            {
                var acceptedByte = buffer[index];
                throw new NotImplementedException("Feed the state automaton");

            }
        }

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
            builder.Emit_RepeatedActionSwitch(headerSwitch);
            return builder.Compile();
        }

        private static Expression _getMethod(AutomatonBuilderContext builder)
        {
            throw new NotImplementedException();
        }

        private static Expression _postMethod(AutomatonBuilderContext builder)
        {
            throw new NotImplementedException();
        }

        private static Expression _putMethod(AutomatonBuilderContext builder)
        {
            throw new NotImplementedException();
        }
    }
}
