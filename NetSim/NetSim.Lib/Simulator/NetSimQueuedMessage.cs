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
        /// Initializes a new instance of the <see cref="NetSimQueuedMessage"/> class.
        /// </summary>
        public NetSimQueuedMessage()
        {
            this.IsRouteDiscoveryStarted = false;
        }

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is route discovery started.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is route discovery started; otherwise, <c>false</c>.
        /// </value>
        public bool IsRouteDiscoveryStarted { get; set; }
    }
}
