using System;
using System.Linq;

namespace NetSim.Lib.Routing.OLSR
{
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
        /// The handle incomming
        /// </summary>
        HandleIncoming
    }
}