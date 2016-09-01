using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvRouteResponseMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the hop count.
        /// </summary>
        /// <value>
        /// The hop count.
        /// </value>
        public int HopCount { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return CopyTo(new AodvRouteResponseMessage()
            {
                HopCount = this.HopCount
            });
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{base.ToString()}\n| HopCount:{HopCount}\n+[/{this.GetType().Name}]";
        }
    }
}
