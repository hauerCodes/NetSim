using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSR
{
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
        /// Gets the next reverse hop.
        /// </summary>
        /// <param name="currentNodeId">The current node identifier.</param>
        /// <returns></returns>
        public string GetNextReverseHop(string currentNodeId)
        {
            if (Route == null || Route.Count == 0)
            {
                return null;
            }

            int searchedIndex = 0;

            searchedIndex = Route.IndexOf(currentNodeId) - 1;

            return searchedIndex < 0 ? null : Route[searchedIndex];
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new DsrRouteReplyMessage()
            {
                Route = new List<string>(this.Route)
            };

            return CopyTo(clone);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{base.ToString()}\n| Route:{string.Join(",", Route)}\n+[/{this.GetType().Name}]";
        }
    }
}
