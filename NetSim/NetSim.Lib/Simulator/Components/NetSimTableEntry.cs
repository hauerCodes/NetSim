// -----------------------------------------------------------------------
// <copyright file="NetSimTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Components
{
    using System;
    using System.Linq;

    /// <summary>
    /// The base class for the table entry implementations.
    /// </summary>
    public abstract class NetSimTableEntry : ICloneable
    {
        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is reachable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is reachable; otherwise, <c>false</c>.
        /// </value>
        public bool IsReachable => this.Metric >= 0;

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public int Metric { get; set; }

        /// <summary>
        /// Gets or sets the next hop.
        /// </summary>
        /// <value>
        /// The next hop.
        /// </value>
        public string NextHop { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public abstract object Clone();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return !this.IsReachable
                       ? $"{this.Destination,4} {this.NextHop,4} {"---",6}"
                       : $"{this.Destination,4} {this.NextHop,4} {this.Metric,6}";
        }
    }
}