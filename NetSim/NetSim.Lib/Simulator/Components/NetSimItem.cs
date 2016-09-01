using System;
using System.Linq;

using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator.Components
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NetSimItem : INetSimVisualizeableItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimItem"/> class.
        /// </summary>
        protected NetSimItem() { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public NetSimLocation Location { get; set; }
    }
}