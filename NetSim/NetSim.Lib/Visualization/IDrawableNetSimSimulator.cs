// -----------------------------------------------------------------------
// <copyright file="IDrawableNetSimSimulator.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - IDrawableNetSimSimulator.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Visualization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The simulator interface for drawing.
    /// </summary>
    public interface IDrawableNetSimSimulator
    {
        /// <summary>
        /// Occurs when simulator gets updated.
        /// </summary>
        event Action SimulatorUpdated;

        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        List<NetSimClient> Clients { get; set; }

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        List<NetSimConnection> Connections { get; set; }
    }
}