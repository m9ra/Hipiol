using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.Events
{
    abstract public class EventBase
    {
        /// <summary>
        /// Action that is used for recycling.
        /// </summary>
        private Action _recycler;

        /// <summary>
        /// Handler that should recycle all fields.
        /// </summary>
        abstract protected void recycle();

        /// <summary>
        /// Accepts given visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        abstract internal void Accept(EventVisitorBase visitor);

        /// <summary>
        /// Set recycler that is used when <see cref="Recycle"/> is called.
        /// </summary>
        /// <param name="recycler">The recycler.</param>
        internal void SetRecycler(Action recycler)
        {
            if (recycler == null)
                throw new ArgumentNullException("recycler");

            if (_recycler != null)
                throw new NotSupportedException("Cannot set recycler twice");

            _recycler = recycler;
        }

        /// <summary>
        /// Runs recyclation routines.
        /// </summary>
        internal void Recycle()
        {
            recycle();
            _recycler();
        }
    }
}
