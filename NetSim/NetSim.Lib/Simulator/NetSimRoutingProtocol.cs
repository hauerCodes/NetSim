using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSim.Lib.Simulator
{
    public abstract class NetSimRoutingProtocol
    {
        /// <summary>
        /// The not reachable
        /// </summary>
        protected const int NotReachable = -1;

        /// <summary>
        /// The client
        /// </summary>
        protected readonly NetSimClient Client;

        /// <summary>
        /// The step counter
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected int stepCounter;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        protected NetSimRoutingProtocol(NetSimClient client)
        {
            this.Client = client;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>
        /// The table.
        /// </value>
        public NetSimTable Table { get; set; }

        /// <summary>
        /// Gets the step counter.
        /// </summary>
        /// <value>
        /// The step counter.
        /// </value>
        public int StepCounter => stepCounter;

        #endregion

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public abstract void PerformRoutingStep();

        /// <summary>
        /// Gets the route.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns></returns>
        public virtual string GetRoute(string destinationId)
        {
            return Table.GetRouteFor(destinationId).NextHop;
        }

        /// <summary>
        /// Checks for topology updates.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckForTopologyUpdates()
        {
            bool topologyChangeUpdate = false; 

            foreach (var connection in Client.Connections.Where(c => c.Value.IsOffline))
            {
                // connection.key is the "to" destination
                var routeEntry = Table.GetRouteFor(connection.Key);

                // if route is reachable - broken link detected
                if (routeEntry != null && routeEntry.IsReachable)
                {
                    routeEntry.Metric = NotReachable;
                    topologyChangeUpdate = true;
                }
            }

            return topologyChangeUpdate;
        }
    }
}