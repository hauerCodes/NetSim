// -----------------------------------------------------------------------
// <copyright file="DsdvTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsdvTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSDV
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The implementation of a dsdv table entry.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTableEntry" />
    public class DsdvTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public DsdvSequence SequenceNr { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned instance of the dsdv table entry.</returns>
        public override object Clone()
        {
            return new DsdvTableEntry()
                   {
                       Destination = this.Destination,
                       Metric = this.Metric,
                       NextHop = this.NextHop,
                       SequenceNr = (DsdvSequence)this.SequenceNr.Clone()
                   };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString() + $" {this.SequenceNr, 5}";
        }
    }
}