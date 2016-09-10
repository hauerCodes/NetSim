// -----------------------------------------------------------------------
// <copyright file="OlsrNeighborTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrNeighborTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The olsr neighbour table entry implementation.
    /// </summary>
    public class OlsrNeighborTableEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrNeighborTableEntry"/> class.
        /// </summary>
        public OlsrNeighborTableEntry()
        {
            this.AccessibleThrough = new List<string>();
        }

        /// <summary>
        /// Gets or sets the accessible though.
        /// </summary>
        /// <value>
        /// The accessible though.
        /// </value>
        public List<string> AccessibleThrough { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi point relay.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is multi point relay; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiPointRelay { get; set; }

        /// <summary>
        /// Gets or sets the neighbor identifier.
        /// </summary>
        /// <value>
        /// The neighbor identifier.
        /// </value>
        public string NeighborId { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.IsMultiPointRelay)
            {
                return $"{this.NeighborId,2} MPR";
            }

            if (this.AccessibleThrough.Any())
            {
                return $"{this.NeighborId,2} {string.Join(",", this.AccessibleThrough)}";
            }

            return this.NeighborId;
        }
    }
}