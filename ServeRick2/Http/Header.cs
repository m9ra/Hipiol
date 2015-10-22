using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

namespace ServeRick2.Http
{
    /// <summary>
    /// Generator for action expression.
    /// </summary>
    /// <param name="request">Expression with stored request.</param>
    /// <param name="currentByte">Byte which caused action execution.</param>
    internal delegate Expression ActionCreator(Expression request, Expression currentByte);

    public abstract class Header<T>
    {
        /// <summary>
        /// Name of the header.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Constant representing 10. Is used for multiplication.
        /// </summary>
        private static readonly Expression _10 = Expression.Constant(10);

        /// <summary>
        /// Template method which builds parser for the header.
        /// </summary>
        protected abstract void buildParser();


        internal Header(string name)
        {
            _name = name;
        }

        #region Parsing utilities

        /// <summary>
        /// Adds input action which is fired for every conditioned byte in the header content.
        /// </summary>
        /// <param name="conditionByte">Byte conditioning the action.</param>
        /// <param name="creator">Creator of the action</param>
        internal void InputAction(byte conditionByte, ActionCreator creator)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Emits input actions to read int into given storage.
        /// </summary>
        /// <param name="intStorage">Storage where int will be stored.</param>
        internal void ReadInt(FieldInfo intStorage)
        {
            for (byte number = 0; number < 10; ++number)
            {
                //parse digits
                InputAction(number, (request, currentByte) =>
                {
                    var storedInt = Expression.Field(request, intStorage);
                    var multipliedInt = Expression.MultiplyAssign(storedInt, _10);
                    var numberToStore = Expression.AddAssign(multipliedInt, currentByte);
                    return numberToStore;
                });
            }
        }

        /// <summary>
        /// Switches action emittion to next state when byte occurs.
        /// <remarks>All actions emitted after state switching will belong to the next state.</remarks>
        /// </summary>
        /// <param name="conditionByte">Byte conditioning the state switching</param>
        internal void SwitchToNextState(byte conditionByte)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
