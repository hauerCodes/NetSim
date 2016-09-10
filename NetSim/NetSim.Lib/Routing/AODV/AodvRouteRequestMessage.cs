// -----------------------------------------------------------------------
// <copyright file="AodvRouteRequestMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvRouteRequestMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The route request message of the protocol.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class AodvRouteRequestMessage : NetSimMessage
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
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public AodvSequence SenderSequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RREQ";

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            var clone = new AodvRouteRequestMessage()
                        {
                            RequestId = this.RequestId,
                            HopCount = this.HopCount,
                            LastHop = this.LastHop,
                            SenderSequenceNr = (AodvSequence)this.SenderSequenceNr.Clone()
                        };

            return this.CopyTo(clone);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(base.ToString());
            builder.AppendFormat("| Request: {0}\n", this.RequestId);
            builder.AppendFormat("| HopCount: {0}\n", this.HopCount);
            builder.AppendFormat("| LastHop: {0}\n", this.LastHop);
            builder.AppendFormat("| Sender SequenceNr: {0}\n", this.SenderSequenceNr);
            builder.AppendLine($"+[/{this.GetType().Name}]");

            return builder.ToString();
        }
    }
}