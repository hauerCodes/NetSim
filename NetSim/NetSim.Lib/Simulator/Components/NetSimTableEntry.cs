using System;
using System.Linq;

namespace NetSim.Lib.Simulator.Components
{
    /// <summary>
    /// 
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
        /// Gets or sets the next hop.
        /// </summary>
        /// <value>
        /// The next hop.
        /// </value>
        public string NextHop { get; set; }

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public int Metric { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is reachable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is reachable; otherwise, <c>false</c>.
        /// </value>
        public bool IsReachable => Metric >= 0;

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return !IsReachable ? $"{Destination, 4} {NextHop, 4} {"---",6}" : $"{Destination,4} {NextHop,4} {Metric,6}";
        }
    }
}