// -----------------------------------------------------------------------
// <copyright file="NetSimMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Components
{
    using System;
    using System.Linq;

    /// <summary>
    /// The message base class for all other messages used in the simulator.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimItem" />
    /// <seealso cref="System.ICloneable" />
    public abstract class NetSimMessage : NetSimItem, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimMessage"/> class.
        /// </summary>
        protected NetSimMessage()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the end receiver of this message.
        /// </summary>
        /// <value>
        /// The receiver.
        /// </value>
        public string Receiver { get; set; }

        /// <summary>
        /// Gets or sets the sender of this message.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public string Sender { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public abstract string ShortName { get; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public abstract object Clone();

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"+[{this.GetType().Name}({this.Sender} -> {this.Receiver})]";
        }

        /// <summary>
        /// Copies the base properties of this message instance to the given message.
        /// </summary>
        /// <param name="copyToMessage">The copy to message.</param>
        /// <returns>The copy message with the filled properties.</returns>
        protected virtual NetSimMessage CopyTo(NetSimMessage copyToMessage)
        {
            copyToMessage.Id = this.Id;
            copyToMessage.Sender = this.Sender;
            copyToMessage.Receiver = this.Receiver;

            return copyToMessage;
        }
    }
}