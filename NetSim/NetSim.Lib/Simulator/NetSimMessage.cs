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
        /// Initializes a new instance of the <see cref="NetSimMessage"/> class.
        /// </summary>
        protected NetSimMessage()
        {
            base.Id = Guid.NewGuid().ToString();
        }

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
        /// Gets or sets the next receiver of the message 
        /// between Endpoint A and Endpoint B (NetSimConnection)
        /// </summary>
        /// <value>
        /// The next receiver.
        /// </value>
        public string NextReceiver { get; set; }

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
        /// Copies the base properties of this message instace to the given message.
        /// </summary>
        /// <param name="copyToMessage">The copy to message.</param>
        /// <returns></returns>
        protected virtual NetSimMessage CopyTo(NetSimMessage copyToMessage)
        {
            copyToMessage.Id = this.Id;
            copyToMessage.Sender = this.Sender;
            copyToMessage.Receiver = this.Receiver;
            copyToMessage.NextReceiver = this.NextReceiver;
            copyToMessage.TransmissionStep = this.TransmissionStep;

            return copyToMessage;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public abstract object Clone();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"#[{this.GetType().Name}] {Sender} - {Receiver}";
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
        }
    }
}