// -----------------------------------------------------------------------
// <copyright file="OlsrTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The olsr table entry implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTableEntry" />
    public class OlsrTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public override object Clone()
        {
            return new OlsrTableEntry() { Destination = this.Destination, Metric = this.Metric, NextHop = this.NextHop };
        }
    }
}