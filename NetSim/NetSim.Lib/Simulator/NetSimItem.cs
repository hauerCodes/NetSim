using System;
using System.Linq;

namespace NetSim.Lib.Simulator
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NetSimItem
    {
        protected NetSimItem()
        {
           
        }

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