// -----------------------------------------------------------------------
// <copyright file="AodvHelloMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvHelloMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// THe Hello message of the protocol.
    /// </summary>
    /// <seealso cref="NetSimMessage" />
    public class AodvHelloMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the not reachable node sequence nr.
        /// </summary>
        /// <value>
        /// The not reachable node sequence nr.
        /// </value>
        public AodvSequence SenderSequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "Hello";

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return this.CopyTo(new AodvHelloMessage() { SenderSequenceNr = (AodvSequence)this.SenderSequenceNr?.Clone() });
        }
    }
}