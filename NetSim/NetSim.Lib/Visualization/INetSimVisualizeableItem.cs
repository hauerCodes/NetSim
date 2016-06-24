using System;
using System.Linq;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Visualization
{
    public interface INetSimVisualizeableItem
    {
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