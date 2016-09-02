using System;
using System.Linq;

namespace NetSim.Lib.Simulator.Components
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        protected NetSimRoutingProtocol(NetSimClient client)
        {
            this.Client = client;
            this.stepCounter = 0;
        }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>
        /// The table.
        /// </value>
        public NetSimTable Table { get; set; }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <value>
        /// The routing data.
        /// </value>
        public string RoutingData => GetRoutingData();

        /// <summary>
        /// Gets the step counter.
        /// </summary>
        /// <value>
        /// The step counter.
        /// </value>
        public int StepCounter => stepCounter;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Initialize()
        {
            // reset stepcounter
            this.stepCounter = 0;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public abstract void PerformRoutingStep();

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void SendMessage(NetSimMessage message);

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetRoutingData();

        /// <summary>
        /// Gets the route.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns></returns>
        protected virtual string GetRoute(string destinationId)
        {
            return Table.GetRouteFor(destinationId)?.NextHop;
        }

        /// <summary>
        /// Determines whether the connection with the specified endpoint identifier is reachable.
        /// Client has this connection (e.g. not deleted) and connection is not offline
        /// </summary>
        /// <param name="destinationId">The endpoint identifier.</param>
        /// <returns>
        ///   <c>true</c> if connection is reachable; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsConnectionReachable(string destinationId)
        {
            return Client?.Connections != null &&
                Client.Connections.ContainsKey(destinationId) &&
                !Client.Connections[destinationId].IsOffline;
        }

    }
}