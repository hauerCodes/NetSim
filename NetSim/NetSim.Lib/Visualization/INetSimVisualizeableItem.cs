using System;
using System.Linq;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Visualization
{
    public interface INetSimVisualizeableItem
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        NetSimLocation Location { get; set; }
    }
}