using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace ServeRick2.Http.Parsing
{
    class AutomatonBuilderContext
    {
        /// <summary>
        /// Variable where actual state is stored.
        /// </summary>
        internal readonly Expression StateVariable = Expression.Variable(typeof(int));

        /// <summary>
        /// Variable where actual input byte is stored.
        /// </summary>
        internal readonly Expression InputVariable = Expression.Variable(typeof(byte));

        /// <summary>
        /// Creates expression reading string into indexed storage ending with given chars.
        /// </summary>
        /// <param name="storageIndex">Index of storage where string will be saved.</param>
        /// <param name="endChars">Characters ending the string.</param>
        /// <returns>Created expression.</returns>
        internal Expression ReadString(int storageIndex, char[] endChars)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates expression passing all data until end of line.
        /// </summary>
        internal Expression PassLine()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates expression which set next state for the automaton.
        /// </summary>
        /// <param name="state">State to set.</param>
        /// <returns>Created expression.</returns>
        internal Expression GoToState(int state)
        {
            return Expression.Block(typeof(void), Expression.Assign(StateVariable, Expression.Constant(state)));
        }
    }
}
