// -----------------------------------------------------------------------
// <copyright file="ConnectionFrameMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - ConnectionFrameMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Messages
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;

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
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => this.InnerMessage?.ShortName ?? string.Empty;

        /// <summary>
        /// Gets or sets the transmission step of this message.
        /// Initial for the initial sending step - going on wire.
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
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return this.CopyTo(new ConnectionFrameMessage() { InnerMessage = this.InnerMessage, });
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{this.GetType().Name} ({this.Sender} -> {this.Receiver})]"
                   + $"\n{this.InnerMessage}\n[/{this.GetType().Name}]";
        }
    }
}