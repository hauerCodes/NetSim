using System;
using System.Linq;

using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Simulator.Messages
{
    /// <summary>
    /// This Message is a connection frame message.
    /// It can be compared with an ethernet frame message.
    /// When transmitting data via a connection a message gets packaged
    /// into this frame.
    /// </summary>
    /// <seealso cref="NetSimMessage" />
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
        /// Gets or sets the transmission step of this message.
        /// Intial for the initial sending step - going on wire.
        /// Transmitting for indicating that the message is on the wire.
        /// Receiving for indicating that the message is going to be received.
        /// </summary>
        /// <value>
        /// The transmission step.
        /// </value>
        public NetSimMessageTransmissionStep TransmissionStep { get; set; } = NetSimMessageTransmissionStep.Sending;

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
            return $"[{this.GetType().Name} ({Sender} -> {Receiver})]"
                   + $"\n{InnerMessage}\n[/{this.GetType().Name}]";
        }
    }
}
