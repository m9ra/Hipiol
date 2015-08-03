using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace Hipiol.Events
{
    /// <summary>
    /// Channel that is used for sending events between processing threads.
    /// Events can be raised in multi-threaded manner. However they are recieved only by single thread.
    /// </summary>
    public abstract class EventChannel<Event>
        where Event : EventBase
    {
        /// <summary>
        /// Lock thread that is used for synchronization with processing thread.
        /// </summary>
        private readonly object _L_ProcessingThread = new object();

        /// <summary>
        /// Queue of events that are waiting for processing.
        /// </summary>
        private List<Event> _waitingEvents = new List<Event>();

        /// <summary>
        /// Queue of events that are already registered by processing thread.
        /// </summary>
        private List<Event> _pendingEvents = new List<Event>();

        /// <summary>
        /// Determine whether processing thread should listen to events.
        /// </summary>
        private volatile bool _shouldListenEvents = false;

        /// <summary>
        /// Determine whether processing thread listens to events.
        /// </summary>
        public bool IsProcessing { get { throw new NotImplementedException("By implementing use volatile variable"); } }

        /// <summary>
        /// Is called on every event that has to be processed.
        /// </summary>
        /// <remarks>Event objects cannot be stored anywhere - they can be recycled after this method ends.</remarks>
        /// <param name="eventObject">Event to be processed.</param>
        protected abstract void Process(Event eventObject);

        /// <summary>
        /// Blocking call that will process araising events in calling thread.
        /// </summary>
        public void ProcessEvents()
        {
            _shouldListenEvents = true;
            while (_shouldListenEvents)
            {
                lock (_L_ProcessingThread)
                {
                    while (_waitingEvents.Count == 0)
                        Monitor.Wait(_L_ProcessingThread);

                    //take all waiting events - we just swap lists because of 
                    //saving element copy operation
                    var swappedEvents = _pendingEvents;
                    _pendingEvents = _waitingEvents;
                    _waitingEvents = swappedEvents;
                }

                foreach (var eventObject in _pendingEvents)
                {

                }
            }
        }

        /// <summary>
        /// Exit from ProcessEvents call of processing thread.
        /// </summary>
        public void StopProcessing()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enqueues event into queue, where it could be processed.
        /// </summary>
        /// <param name="eventObject">Event that is enqueued</param>
        public void EnqueueEvent(Event eventObject)
        {
            lock (_L_ProcessingThread)
            {
                _waitingEvents.Add(eventObject);
                Monitor.Pulse(_L_ProcessingThread);
            }
        }
    }
}
