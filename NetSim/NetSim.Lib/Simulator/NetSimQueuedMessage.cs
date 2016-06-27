using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Simulator
{
    public class NetSimQueuedMessage
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public NetSimMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the route for the message.
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        public NetSimTableEntry Route { get; set; }
    }
}
