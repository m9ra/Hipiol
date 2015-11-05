using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace ServeRick2.Http.Parsing
{
    class AutomatonState
    {
        /// <summary>
        /// Id of current state.
        /// </summary>
        internal readonly int Id;

        /// <summary>
        /// Targets indexed by triggering bytes.
        /// </summary>
        private readonly Dictionary<byte, AutomatonState> _byteTargets = new Dictionary<byte, AutomatonState>();

        /// <summary>
        /// Target state that will be active after state action is processed.
        /// </summary>
        private AutomatonState _targetState;

        /// <summary>
        /// Director that is used for building of state action.
        /// </summary>
        private AutomatonBuilderDirector _director;

        internal AutomatonState(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Sets action for current state, ending at target state.
        /// </summary>
        /// <param name="stateActionDirector">Director for building the action.</param>
        /// <param name="targetState">Target which will be active after the action is processed.</param>
        internal void SetAction(AutomatonBuilderDirector stateActionDirector, AutomatonState targetState)
        {
            if (_byteTargets.Count > 0)
                throw new NotSupportedException("Cannot set state action when byte target is available");

            if (stateActionDirector == null)
                throw new ArgumentNullException("stateActionDirector");

            if (targetState == null)
                throw new ArgumentNullException("targetState");

            _director = stateActionDirector;
            _targetState = targetState;
        }

        /// <summary>
        /// Determine whether state contains target state for given byte.
        /// </summary>
        /// <param name="b">Condition byte.</param>
        /// <returns><c>true</c> if target is contained, <c>false</c> otherwise.</returns>
        internal bool HasByteTarget(byte b)
        {
            return _byteTargets.ContainsKey(b);
        }

        /// <summary>
        /// Sets target for given byte input.
        /// </summary>
        /// <param name="b">Condition byte.</param>
        /// <param name="target">Target state.</param>
        internal void SetByteTarget(byte b, AutomatonState target)
        {
            if (_director != null)
                throw new NotImplementedException("cannot set byte target when state action is present");

            _byteTargets.Add(b, target);
        }

        /// <summary>
        /// Gets target for given byte input.
        /// </summary>
        /// <param name="b">Condition byte.</param>
        /// <returns>Target state.</returns>
        internal AutomatonState GetByteTarget(byte b)
        {
            return _byteTargets[b];
        }

        /// <summary>
        /// Compiles state into <see cref="SwitchCase"/> expression.
        /// </summary>
        /// <param name="context">Context with compilation info.</param>
        /// <returns>Compiled state.</returns>
        internal SwitchCase Compile(AutomatonBuilderContext context)
        {
            var stateBody = compileStateBody(context);
            
            return Expression.SwitchCase(stateBody, Expression.Constant(Id));
        }

        private Expression compileStateBody(AutomatonBuilderContext context)
        {
            if (_director != null)
                //we can directly compile the action
                return _director(context);


            //compile byte switch table
            var byteTargetCases = new List<SwitchCase>();
            foreach (var byteTargetPair in _byteTargets)
            {
                var gotoStateExpression = context.GoToState(byteTargetPair.Value.Id);

                var byteTargetCase = Expression.SwitchCase(gotoStateExpression, Expression.Constant(byteTargetPair.Key));
                byteTargetCases.Add(byteTargetCase);
            }

            return Expression.Switch(context.InputVariable, byteTargetCases.ToArray());
        }
    }
}
