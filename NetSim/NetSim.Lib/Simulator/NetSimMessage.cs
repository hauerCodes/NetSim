using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public abstract class NetSimMessage : NetSimItem, ICloneable
    {
        /// <summary>
        /// Gets or sets the sender of this message.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the end receiver of this message.
        /// </summary>
        /// <value>
        /// The receiver.
        /// </value>
        public string Receiver { get; set; }

        /// <summary>
        /// Gets or sets the next receiver of the message between Endpoint A and Endpoint B (NetSimConnection)
        /// </summary>
        /// <value>
        /// The next receiver.
        /// </value>
        public string NextReceiver { get; set; }

        public abstract object Clone();

        public override string ToString()
        {
            return $"#[{this.GetType().Name}] {Sender} - {Receiver}";
        }
    }
}