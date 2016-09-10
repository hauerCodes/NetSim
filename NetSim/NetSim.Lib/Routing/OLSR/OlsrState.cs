// -----------------------------------------------------------------------
// <copyright file="OlsrState.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrState.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Linq;

    /// <summary>
    /// The olsr state enumeration.
    /// </summary>
    public enum OlsrState
    {
        /// <summary>
        /// The hello
        /// </summary>
        Hello,

        /// <summary>
        /// The receive hello
        /// </summary>
        ReceiveHello,

        /// <summary>
        /// The calculate
        /// </summary>
        Calculate,

        /// <summary>
        /// The topology control
        /// </summary>
        TopologyControl,

        /// <summary>
        /// The handle incoming
        /// </summary>
        HandleIncoming
    }
}