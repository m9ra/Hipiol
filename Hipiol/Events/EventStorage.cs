using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.Events
{

    /// <summary>
    /// Storage that handles recycling of <see cref="EventBase"/>.
    /// </summary>
    /// <typeparam name="Event"></typeparam>
    class EventStorage<Event> 
        where Event : EventBase, new()
    {
        /// <summary>
        /// Lock which allows multithreaded recycling.
        /// </summary>
        private readonly object _L_events = new object();

        /// <summary>
        /// Free events that are available for use.
        /// </summary>
        private readonly Stack<Event> _events = new Stack<Event>();

        /// <summary>
        /// Gets (or creates) event.
        /// </summary>
        /// <returns>The event.</returns>
        internal Event GetEvent()
        {
            lock (_L_events)
            {
                if (_events.Count > 0)
                    return _events.Pop();
            }

            var newEvent = new Event();
            newEvent.SetRecycler(() => recycle(newEvent));

            return newEvent;
        }

        private void recycle(Event evt)
        {
            lock (_L_events)
            {
                _events.Push(evt);
            }
        }
    }
}
