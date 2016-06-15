using System;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace NetSim.Lib.Simulator
{
    public enum NetSimProtocol
    {
        /// <summary>
        /// Destionation Sequences Distance Vector
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