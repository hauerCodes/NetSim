using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Simulator
{
    /// <summary>
    /// This Message is a connection frame message.
    /// It can be compared with an ethernet frame message.
    /// When transmitting data via a connection a message gets packaged
    /// into this frame.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.NetSimMessage" />
    public class ConnectionFrameMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public NetSimMessage InnerMessage { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return CopyTo(new ConnectionFrameMessage()
            {
                InnerMessage = this.InnerMessage,
            });
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{this.GetType().Name}]\n[{Sender} - {Receiver}]"
                   + $"\n----{InnerMessage}\n----[{this.GetType().Name}]";
        }
    }
}
