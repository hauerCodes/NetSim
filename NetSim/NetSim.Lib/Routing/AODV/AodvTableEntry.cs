// -----------------------------------------------------------------------
// <copyright file="AodvTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The aodv table entry implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTableEntry" />
    /// <seealso cref="NetSimTableEntry" />
    public class AodvTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AodvTableEntry"/> class.
        /// </summary>
        public AodvTableEntry()
        {
            this.ActiveNeighbours = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets or sets the active neighbors dictionary. 
        /// The key identifies the neighbor. 
        /// The value part is a time counter.
        /// </summary>
        /// <value>
        /// The active neighbors.
        /// </value>
        public Dictionary<string, int> ActiveNeighbours { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public AodvSequence SequenceNr { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public override object Clone()
        {
            return new AodvTableEntry()
            {
                Destination = this.Destination,
                Metric = this.Metric,
                NextHop = this.NextHop,
                SequenceNr = (AodvSequence)this.SequenceNr.Clone()
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
            return base.ToString() + $" {this.SequenceNr,5} {string.Join(",", this.ActiveNeighbours.Keys)}";
        }
    }
}