// -----------------------------------------------------------------------
// <copyright file="DsrRouteErrorMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsrRouteErrorMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsr route error message implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class DsrRouteErrorMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the failed message.
        /// </summary>
        /// <value>
        /// The failed message.
        /// </value>
        public NetSimMessage FailedMessage { get; set; }

        /// <summary>
        /// Gets or sets the not reachable node.
        /// </summary>
        /// <value>
        /// The not reachable node.
        /// </value>
        public string NotReachableNode { get; set; }

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public List<string> Route { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RERR";

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public override object Clone()
        {
            var clone = new DsrRouteErrorMessage()
            {
                NotReachableNode = this.NotReachableNode,
                FailedMessage = (NetSimMessage)this.FailedMessage?.Clone()
            };

            if (this.Route != null)
            {
                clone.Route = new List<string>(this.Route);
            }

            return this.CopyTo(clone);
        }

        /// <summary>
        /// Gets the next reverse hop.
        /// </summary>
        /// <param name="currentNodeId">The current node identifier.</param>
        /// <returns>The next reverse hop on the saved route or null.</returns>
        public string GetNextReverseHop(string currentNodeId)
        {
            if (this.Route == null || this.Route.Count == 0)
            {
                return null;
            }

            var searchedIndex = this.Route.IndexOf(currentNodeId) - 1;

            return searchedIndex < 0 ? null : this.Route[searchedIndex];
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(base.ToString());

            builder.AppendFormat("| NotReachable:{0}\n", this.NotReachableNode);

            if (this.Route != null && this.Route.Count > 0)
            {
                builder.AppendFormat("| Route:{0}\n", string.Join(",", this.Route));
            }

            if (this.FailedMessage != null)
            {
                builder.AppendFormat("| FailedMessage:\n #| {0}\n", this.FailedMessage);
            }

            builder.AppendFormat("+[/{0}]", this.GetType().Name);

            return builder.ToString();
        }
    }
}