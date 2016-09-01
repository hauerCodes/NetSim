using System;
using System.Collections.Generic;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Visualization
{
    /// <summary>
    /// 
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