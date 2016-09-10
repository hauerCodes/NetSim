// -----------------------------------------------------------------------
// <copyright file="AodvRouteReplyMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvRouteReplyMessage.cs</summary>
// -----------------------------------------------------------------------
namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The route reply message of the protocol.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class AodvRouteReplyMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the hop count.
        /// </summary>
        /// <value>
        /// The hop count.
        /// </value>
        public int HopCount { get; set; }

        /// <summary>
        /// Gets or sets the last hop.
        /// Note: This Info has to be saved to determine from 
        /// which interface or hop this message has been sent 
        /// </summary>
        /// <value>
        /// The last hop.
        /// </value>
        public string LastHop { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public AodvSequence ReceiverSequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RREP";

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return
                this.CopyTo(
                    new AodvRouteReplyMessage()
                    {
                        HopCount = this.HopCount,
                        ReceiverSequenceNr = (AodvSequence)this.ReceiverSequenceNr.Clone()
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
            return $"{base.ToString()}\n| HopCount:{this.HopCount}\n+[/{this.GetType().Name}]";
        }
    }
}