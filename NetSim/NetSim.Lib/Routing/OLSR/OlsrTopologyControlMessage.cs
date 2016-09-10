// -----------------------------------------------------------------------
// <copyright file="OlsrTopologyControlMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrTopologyControlMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The olsr topology control message implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class OlsrTopologyControlMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrTopologyControlMessage"/> class.
        /// </summary>
        public OlsrTopologyControlMessage()
        {
            this.MultiPointRelaySelectorSet = new List<string>();
        }

        /// <summary>
        /// Gets or sets the multi point relays.
        /// </summary>
        /// <value>
        /// The multi point relays.
        /// </value>
        public List<string> MultiPointRelaySelectorSet { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public OlsrSequence SequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "TC";

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            var clone = new OlsrTopologyControlMessage()
            {
                SequenceNr = (OlsrSequence)this.SequenceNr?.Clone(),
                MultiPointRelaySelectorSet = new List<string>(this.MultiPointRelaySelectorSet)
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

            if (this.SequenceNr != null)
            {
                builder.AppendFormat("| SequenceNr: {0}\n", this.SequenceNr);
            }

            if (this.MultiPointRelaySelectorSet != null && this.MultiPointRelaySelectorSet.Count > 0)
            {
                builder.AppendFormat("| MPR-SelectorSet: {0}\n", string.Join(",", this.MultiPointRelaySelectorSet));
            }

            builder.AppendLine($"+[/{this.GetType().Name}]");

            return builder.ToString();
        }
    }
}