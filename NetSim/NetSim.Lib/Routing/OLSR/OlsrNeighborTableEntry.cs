using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrNeighborTableEntry
    {
        public OlsrNeighborTableEntry()
        {
            this.AccessableThrough = new List<string>();
        }

        /// <summary>
        /// Gets or sets the neighbor identifier.
        /// </summary>
        /// <value>
        /// The neighbor identifier.
        /// </value>
        public string NeighborId { get; set; }

        /// <summary>
        /// Gets or sets the accessable though.
        /// </summary>
        /// <value>
        /// The accessable though.
        /// </value>
        public List<string> AccessableThrough { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi point relay.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is multi point relay; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiPointRelay { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsMultiPointRelay)
            {
                return $"{NeighborId,2} MPR";
            }

            if (AccessableThrough.Any())
            {
                return $"{NeighborId,2} {String.Join(",", AccessableThrough)}";
            }

            return NeighborId;
        }
    }
}