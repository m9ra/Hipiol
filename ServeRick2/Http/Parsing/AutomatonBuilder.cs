using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace ServeRick2.Http.Parsing
{
    class AutomatonBuilder
    {
        /// <summary>
        /// Context which is used for building the automaton.
        /// </summary>
        private readonly AutomatonBuilderContext _context;

        /// <summary>
        /// Root state of the automaton.
        /// </summary>
        private readonly AutomatonState _rootState;

        /// <summary>
        /// State that was used as last target state.
        /// </summary>
        private AutomatonState _lastTargetState;

        /// <summary>
        /// State that is used repeatedly as a target.
        /// </summary>
        private AutomatonState _repeatedTargetState;

        internal AutomatonBuilder()
        {
            _context = new AutomatonBuilderContext();
            _rootState = _context.CreateNewState();
            _lastTargetState = _rootState;
        }

        /// <summary>
        /// Emits directed actions conditioned by indexing strings.
        /// </summary>
        /// <param name="actions">Actions to emit.</param>
        internal void Emit_ActionSwitch(Dictionary<string, AutomatonBuilderDirector> actions, AutomatonBuilderDirector defaultConditionAction = null)
        {
            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            AutomatonState defaultConditionState = null;
            if (defaultConditionAction != null)
            {
                defaultConditionState = _context.CreateNewState();
                defaultConditionState.SetAction(defaultConditionAction, targetState);
            }

            setConditionedActions(currentState, targetState, actions, defaultConditionState);
        }

        /// <summary>
        /// Emits directed actions conditioned by indexing strings repeating until empty line.
        /// </summary>
        /// <param name="actions">Actions to emit.</param>
        internal void Emit_RepeatedActionSwitch(Dictionary<string, AutomatonBuilderDirector> actions, AutomatonBuilderDirector defaultConditionAction = null)
        {
            //set repeating of target state
            _repeatedTargetState = _lastTargetState;

            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            AutomatonState defaultConditionState = null;
            if (defaultConditionAction != null)
            {
                defaultConditionState = _context.CreateNewState();
                defaultConditionState.SetAction(defaultConditionAction, targetState);
            }


            //setting current state as output state will cause repeating of the state.
            setConditionedActions(currentState, currentState, actions, defaultConditionState);
        }

        /// <summary>
        /// Emits action conditioned by given byte in current target state.
        /// </summary>
        /// <param name="condition">The condition byte.</param>
        /// <param name="action">The action.</param>
        internal void Emit_TargetAction(byte condition, AutomatonBuilderDirector action)
        {
            _lastTargetState.SetByteAction(condition, action);
        }

        /// <summary>
        /// Emits blob reading into indexed storage ending with given chars.
        /// </summary>
        /// <param name="storageIndex">Index of storage where string will be saved.</param>
        /// <param name="endChars">Characters ending the string.</param>
        internal void Emit_ExclusiveReadBlob(int storageIndex, params char[] endChars)
        {
            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            currentState.SetAction((c) => c.ExclusiveReadBlob(storageIndex, endChars), targetState);
        }

        /// <summary>
        /// Emits passing all data until end of line.
        /// </summary>
        internal void Emit_PassLine()
        {
            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            currentState.SetAction((c) => c.PassLine(), targetState);
        }

        /// <summary>
        /// Compiles specified automaton.
        /// </summary>
        /// <returns>The automaton.</returns>
        internal ParsingAutomaton Compile()
        {
            //last target state has to be filled
            if (_lastTargetState.IsEmpty)
                _lastTargetState.SetAction((c) => Expression.Break(c.EndInputReading), null);

            //compile all states
            var stateActions = new List<SwitchCase>();
            foreach (var state in _context.RegisteredStates)
            {
                var stateCase = state.Compile(_context);
                stateActions.Add(stateCase);
            }

            var stateSwitch = Expression.Switch(_context.StateStorage, stateActions.ToArray());
            var automatonExpression = _context.IterateInput(stateSwitch);

            return Expression.Lambda<ParsingAutomaton>(automatonExpression, _context.RequestParameter).Compile();
        }

        #region Private utilities

        /// <summary>
        /// Prepares new state for emission and target state.
        /// </summary>
        /// <param name="currentState">State prepared for emission.</param>
        /// <param name="targetState">State that will be used as a target when emitted action is completed.</param>
        private void prepareNewState(out AutomatonState currentState, out AutomatonState targetState)
        {
            //we will begin with state that was used as last target.
            currentState = _lastTargetState;
            //pre-create next state as a target

            if (_repeatedTargetState == null)
            {
                targetState = _context.CreateNewState();
                _lastTargetState = targetState;
            }
            else
            {
                //target is repeated
                targetState = _lastTargetState;
            }
        }

        /// <summary>
        /// Finds/creates a state representing condition occurence on startState.
        /// </summary>
        /// <param name="startState">State from which condition is tested.</param>
        /// <param name="condition">Tested condition.</param>
        /// <returns>The conditioned state.</returns>
        private AutomatonState getActionState(AutomatonState startState, string condition, AutomatonState defaultActionState = null)
        {
            var currentState = startState;
            for (var i = 0; i < condition.Length; ++i)
            {
                //find/create state path corresponding with the condition
                var ch = condition[i];
                var byteLower = (byte)char.ToLower(ch);
                var byteUpper = (byte)char.ToUpper(ch);
                if (!currentState.HasByteTarget(byteLower))
                {
                    var nextState = _context.CreateNewState();
                    currentState.SetByteTarget(byteLower, nextState);
                    currentState.SetByteTarget(byteUpper, nextState);
                }

                currentState.SetDefaultByteTarget(defaultActionState);
                currentState = currentState.GetByteTarget(byteLower);
            }

            return currentState;
        }

        /// <summary>
        /// Sets conditioned actions to current state.
        /// </summary>
        /// <param name="currentState">The state which actions will be set.</param>
        /// <param name="targetState">The target state of actions.</param>
        /// <param name="actions">The actions to be set.</param>
        /// <param name="defaultActionState">If available it is a default state used when conditions won't match.</param>
        private void setConditionedActions(AutomatonState currentState, AutomatonState targetState, Dictionary<string, AutomatonBuilderDirector> actions, AutomatonState defaultActionState)
        {
            foreach (var actionPair in actions)
            {
                var actionState = getActionState(currentState, actionPair.Key, defaultActionState);
                actionState.SetAction(actionPair.Value, targetState);
            }
        }

        #endregion
    }
}
