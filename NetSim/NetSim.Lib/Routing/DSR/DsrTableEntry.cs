// -----------------------------------------------------------------------
// <copyright file="DsrTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsrTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsr table entry implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTableEntry" />
    public class DsrTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrTableEntry"/> class.
        /// </summary>
        public DsrTableEntry()
        {
            this.Route = new List<string>();
        }

        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        public List<string> Route { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public override object Clone()
        {
            return new DsrTableEntry() { Destination = this.Destination, Route = new List<string>(this.Route) };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return !this.IsReachable
                       ? $"{this.Destination,4} {"---",6}"
                       : $"{this.Destination,4} {this.Metric,5} {string.Join(",", this.Route)}";
        }
    }
}