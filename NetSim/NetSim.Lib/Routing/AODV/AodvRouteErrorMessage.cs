// -----------------------------------------------------------------------
// <copyright file="AodvRouteErrorMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvRouteErrorMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The route error message of the protocol.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class AodvRouteErrorMessage : NetSimMessage
    {
        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RERR";

        /// <summary>
        /// Gets or sets the not reachable node.
        /// </summary>
        /// <value>
        /// The not reachable node.
        /// </value>
        public string UnReachableDestination { get; set; }

        /// <summary>
        /// Gets or sets the not reachable node sequence nr.
        /// </summary>
        /// <value>
        /// The not reachable node sequence nr.
        /// </value>
        public AodvSequence UnReachableDestinationSequenceNr { get; set; }

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
                    new AodvRouteErrorMessage()
                    {
                        UnReachableDestination = this.UnReachableDestination,
                        UnReachableDestinationSequenceNr = (AodvSequence)this.UnReachableDestinationSequenceNr?.Clone(),
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
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(base.ToString());

            builder.AppendFormat("| Unreachable Destination:{0}\n", this.UnReachableDestination);

            if (this.UnReachableDestinationSequenceNr != null)
            {
                builder.AppendFormat("| Unreachable SequenceNr:{0}\n", this.UnReachableDestinationSequenceNr);
            }

            builder.AppendFormat("+[/{0}]", this.GetType().Name);

            return builder.ToString();
        }
    }
}