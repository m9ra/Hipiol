using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick2.Http.Parsing
{
    class AutomatonSwitch
    {
        /// <summary>
        /// Actions that are handled by the switch.
        /// </summary>
        private readonly Dictionary<string, AutomatonBuilderDirector> _actions = new Dictionary<string, AutomatonBuilderDirector>();

        /// <summary>
        /// Determines whether switch execution will be repated for all remaining lines.
        /// </summary>
        internal bool IsRepeating;

        /// <summary>
        /// Fills the switch with given actions.
        /// </summary>
        /// <param name="actions">Actions for filling the switch.</param>
        internal void FillWith(Dictionary<string, AutomatonBuilderDirector> actions)
        {
            foreach (var actionPair in actions)
            {
                _actions.Add(actionPair.Key, actionPair.Value);
            }
        }
    }
}
