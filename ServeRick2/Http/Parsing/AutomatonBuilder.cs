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
        /// States that has been registered.
        /// </summary>
        private readonly List<AutomatonState> _registeredStates = new List<AutomatonState>();

        /// <summary>
        /// Root state of the automaton.
        /// </summary>
        private readonly AutomatonState _rootState;

        /// <summary>
        /// State that was used as last target state.
        /// </summary>
        private AutomatonState _lastTargetState;

        internal AutomatonBuilder()
        {
            _context = new AutomatonBuilderContext();
            _rootState = createNewState();
            _lastTargetState = _rootState;
        }

        /// <summary>
        /// Emits directed actions conditioned by indexing strings.
        /// </summary>
        /// <param name="actions">Actions to emit.</param>
        internal void Emit_ActionSwitch(Dictionary<string, AutomatonBuilderDirector> actions)
        {
            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            setConditionedActions(currentState, targetState, actions);
        }

        /// <summary>
        /// Emits directed actions conditioned by indexing strings repeating until empty line.
        /// </summary>
        /// <param name="actions">Actions to emit.</param>
        internal void Emit_RepeatedActionSwitch(Dictionary<string, AutomatonBuilderDirector> actions)
        {
            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            //setting current state as output state will cause repeating of the state.
            setConditionedActions(currentState, currentState, actions);
        }

        /// <summary>
        /// Emits string reading into indexed storage ending with given chars.
        /// </summary>
        /// <param name="storageIndex">Index of storage where string will be saved.</param>
        /// <param name="endChars">Characters ending the string.</param>
        internal void Emit_ReadString(int storageIndex, params char[] endChars)
        {
            AutomatonState currentState;
            AutomatonState targetState;
            prepareNewState(out currentState, out targetState);

            currentState.SetAction((c) => c.ReadString(storageIndex, endChars), targetState);
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
            //compile all states
            var stateActions = new List<SwitchCase>();
            foreach (var statePair in _registeredStates)
            {
                var stateCase = statePair.Compile(_context);
            }


            var stateSwitch = Expression.Switch(_context.StateVariable, stateActions.ToArray());

            throw new NotImplementedException();
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
            targetState = createNewState();
            _lastTargetState = targetState;
        }

        /// <summary>
        /// Creates new state for constructed automaton.
        /// </summary>
        /// <returns>Created state.</returns>
        private AutomatonState createNewState()
        {
            var state = new AutomatonState(_registeredStates.Count);
            _registeredStates.Add(state);

            return state;
        }

        /// <summary>
        /// Finds/creates a state representing condition occurence on startState.
        /// </summary>
        /// <param name="startState">State from which condition is tested.</param>
        /// <param name="condition">Tested condition.</param>
        /// <returns>The conditioned state.</returns>
        private AutomatonState getActionState(AutomatonState startState, string condition)
        {
            var currentState = startState;
            for (var i = 0; i < condition.Length; ++i)
            {
                //find/create state path corresponding with the condition
                var ch = (byte)condition[i];
                if (!currentState.HasByteTarget(ch))
                    currentState.SetByteTarget(ch, createNewState());

                currentState = currentState.GetByteTarget(ch);
            }

            return currentState;
        }

        /// <summary>
        /// Sets conditioned actions to current state.
        /// </summary>
        /// <param name="currentState">The state which actions will be set.</param>
        /// <param name="targetState">The target state of actions.</param>
        /// <param name="actions">The actions to be set.</param>
        private void setConditionedActions(AutomatonState currentState, AutomatonState targetState, Dictionary<string, AutomatonBuilderDirector> actions)
        {
            foreach (var actionPair in actions)
            {
                var actionState = getActionState(currentState, actionPair.Key);
                actionState.SetAction(actionPair.Value, targetState);
            }
        }

        #endregion
    }
}
