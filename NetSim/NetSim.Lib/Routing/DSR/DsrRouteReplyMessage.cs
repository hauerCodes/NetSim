// -----------------------------------------------------------------------
// <copyright file="DsrRouteReplyMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsrRouteReplyMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsr route reply message.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class DsrRouteReplyMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrRouteReplyMessage"/> class.
        /// </summary>
        public DsrRouteReplyMessage()
        {
            this.Route = new List<string>();
        }

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
        public override string ShortName => "RREP";

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public override object Clone()
        {
            var clone = new DsrRouteReplyMessage() { Route = new List<string>(this.Route) };

            return this.CopyTo(clone);
        }

        /// <summary>
        /// Gets the next reverse hop.
        /// </summary>
        /// <param name="currentNodeId">The current node identifier.</param>
        /// <returns>The next reverse route hop on the saved route of the message or null.</returns>
        public string GetNextReverseHop(string currentNodeId)
        {
            if (this.Route == null || this.Route.Count == 0)
            {
                return null;
            }

            int searchedIndex = 0;

            searchedIndex = this.Route.IndexOf(currentNodeId) - 1;

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
            return $"{base.ToString()}\n| Route:{string.Join(",", this.Route)}\n+[/{this.GetType().Name}]";
        }
    }
}