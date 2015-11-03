using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http.Parsing
{
    class AutomatonBuilder
    {
        /// <summary>
        /// Switches (ordered as they will appear in the resulting code).
        /// </summary>
        private readonly List<AutomatonSwitch> _registeredSwitches = new List<AutomatonSwitch>();

        /// <summary>
        /// States that has been registered.
        /// </summary>
        private readonly Dictionary<int, AutomatonBuilderDirector> _registeredStates = new Dictionary<int, AutomatonBuilderDirector>();

        /// <summary>
        /// Emits directed actions conditioned by indexing strings.
        /// </summary>
        /// <param name="actions">Actions to emit.</param>
        internal void Emit_ActionSwitch(Dictionary<string, AutomatonBuilderDirector> actions)
        {
            var actionSwitch = createNextSwitch();
            actionSwitch.FillWith(actions);
        }

        /// <summary>
        /// Emits directed actions conditioned by indexing strings repeating until empty line.
        /// </summary>
        /// <param name="actions">Actions to emit.</param>
        internal void Emit_RepeatedActionSwitch(Dictionary<string, AutomatonBuilderDirector> actions)
        {
            Emit_RepeatedActionSwitch(actions);
            _registeredSwitches.Last().IsRepeating = true;
        }

        /// <summary>
        /// Emits string reading into indexed storage ending with given chars.
        /// </summary>
        /// <param name="storageIndex">Index of storage where string will be saved.</param>
        /// <param name="endChars">Characters ending the string.</param>
        internal void Emit_ReadString(int storageIndex, params char[] endChars)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Emits passing all data until end of line.
        /// </summary>
        internal void Emit_PassLine()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compiles specified automaton.
        /// </summary>
        /// <returns>The automaton.</returns>
        internal ParsingAutomaton Compile()
        {
            throw new NotImplementedException();
        }

        #region Private utilities

        /// <summary>
        /// Creates new automaton state.
        /// </summary>
        /// <returns>Created state.</returns>
        private AutomatonSwitch createNextSwitch()
        {
            var state = new AutomatonSwitch();

            _registeredSwitches.Add(state);
            return state;
        }

        #endregion
    }
}
