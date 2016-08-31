using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Routing.DSR
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method)]
    public class DsrMessageHandlerAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DsrMessageHandlerAttribute"/> class.
        /// </summary>
        /// <param name="messageToHandle">The message to handle.</param>
        /// <param name="outgoing">if set to <c>true</c> [outgoing].</param>
        public DsrMessageHandlerAttribute(Type messageToHandle, bool outgoing = false)
        {
            this.MessageToHandle = messageToHandle;
            this.Outgoing = outgoing;
        }

        /// <summary>
        /// Gets or sets the message to handle.
        /// </summary>
        /// <value>
        /// The message to handle.
        /// </value>
        public Type MessageToHandle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsrMessageHandlerAttribute"/> is outgoing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if outgoing; otherwise, <c>false</c>.
        /// </value>
        public bool Outgoing { get; set; } = false;

    }
}
