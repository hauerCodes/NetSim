using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrRouteErrorMessage : NetSimMessage
    {

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public List<string> Route { get; set; }

        /// <summary>
        /// Gets or sets the not reachable node.
        /// </summary>
        /// <value>
        /// The not reachable node.
        /// </value>
        public string NotReachableNode { get; set; }

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
            var clone = new DsrRouteErrorMessage()
            {
                NotReachableNode = this.NotReachableNode,
                Route = new List<string>(this.Route)
            };

            return CopyTo(clone);
        }
    }
}
