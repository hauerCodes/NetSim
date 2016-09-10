// -----------------------------------------------------------------------
// <copyright file="NetSimProtocolType.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimProtocolType.cs</summary>
// -----------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace NetSim.Lib.Simulator
{
    using System;
    using System.Linq;

    /// <summary>
    /// The protocol type enumeration.
    /// </summary>
    public enum NetSimProtocolType
    {
        /// <summary>
        /// Destination Sequences Distance Vector
        /// </summary>
        DSDV,

        /// <summary>
        /// Ad-Hoc On-Demand Distance Vector
        /// </summary>
        AODV,

        /// <summary>
        /// Optimized Link State Routing
        /// </summary>
        OLSR,

        /// <summary>
        /// Dynamic Source Routing
        /// </summary>
        DSR
    }
}