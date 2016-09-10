// -----------------------------------------------------------------------
// <copyright file="DsrFrameMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsrFrameMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsr frame message implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class DsrFrameMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrFrameMessage"/> class.
        /// </summary>
        public DsrFrameMessage()
        {
            this.Route = new List<string>();
        }

        /// <summary>
        /// Gets or sets the data message.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public NetSimMessage Data { get; set; }

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
        public override string ShortName => this.Data.ShortName;

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            var clone = new DsrFrameMessage()
                        {
                            Route = new List<string>(this.Route),
                            Data = (NetSimMessage)this.Data.Clone()
                        };

            return this.CopyTo(clone);
        }

        /// <summary>
        /// Gets the next hop.
        /// </summary>
        /// <param name="currentNodeId">The current node identifier.</param>
        /// <returns>The next hop in the saved route of the message or null.</returns>
        public string GetNextHop(string currentNodeId)
        {
            if (this.Route == null || this.Route.Count == 0)
            {
                return null;
            }

            var searchedIndex = this.Route.IndexOf(currentNodeId) + 1;

            if (searchedIndex < 0)
            {
                return null;
            }

            return this.Route[searchedIndex];
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{this.GetType().Name} ({this.Sender} -> {this.Receiver})]"
                   + $"\n{this.Data}\n[/{this.GetType().Name}]";
        }
    }
}