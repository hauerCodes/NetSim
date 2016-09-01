using System;
using System.Linq;

using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Simulator.Components
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
        /// Copies the base properties of this message instace to the given message.
        /// </summary>
        /// <param name="copyToMessage">The copy to message.</param>
        /// <returns></returns>
        protected virtual NetSimMessage CopyTo(NetSimMessage copyToMessage)
        {
            copyToMessage.Id = this.Id;
            copyToMessage.Sender = this.Sender;
            copyToMessage.Receiver = this.Receiver;
          
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
            return $"+[{this.GetType().Name}({Sender} -> {Receiver})]";
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