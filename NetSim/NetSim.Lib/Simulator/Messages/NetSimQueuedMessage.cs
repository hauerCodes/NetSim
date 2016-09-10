// -----------------------------------------------------------------------
// <copyright file="NetSimQueuedMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimQueuedMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Messages
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The Queued message is used in protocols which have a route discovery mechanism.
    /// </summary>
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
        /// Gets or sets a value indicating whether this instance is route discovery started.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is route discovery started; otherwise, <c>false</c>.
        /// </value>
        public bool IsRouteDiscoveryStarted { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public NetSimMessage Message { get; set; }
    }
}