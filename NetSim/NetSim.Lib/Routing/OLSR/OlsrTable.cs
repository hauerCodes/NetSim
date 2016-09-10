// -----------------------------------------------------------------------
// <copyright file="OlsrTable.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrTable.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The olsr table implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTable" />
    public class OlsrTable : NetSimTable
    {
        /// <summary>
        /// Adds the initial route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        public void AddRouteEntry(string destination, string nextHop, int metric)
        {
            this.Entries.Add(new OlsrTableEntry() { Destination = destination, NextHop = nextHop, Metric = metric, });
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public override object Clone()
        {
            return new OlsrTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
        }

        /// <summary>
        /// Gets the route for.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns>
        /// The found route for the destination or null.
        /// </returns>
        public override NetSimTableEntry GetRouteFor(string destinationId)
        {
            return this.Entries.FirstOrDefault(e => e.Destination.Equals(destinationId));
        }
    }
}