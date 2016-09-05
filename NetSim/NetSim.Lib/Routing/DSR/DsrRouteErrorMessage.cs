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
        /// Gets or sets the failed message.
        /// </summary>
        /// <value>
        /// The failed message.
        /// </value>
        public NetSimMessage FailedMessage { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RERR";

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
                FailedMessage = (NetSimMessage)this.FailedMessage?.Clone()
            };

            if (this.Route != null)
            {
                clone.Route = new List<string>(this.Route);
            }

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
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(base.ToString());

            builder.AppendFormat("| NotReachable:{0}\n", NotReachableNode);

            if (Route != null && Route.Count > 0)
            {
                builder.AppendFormat("| Route:{0}\n", string.Join(",", Route));
            }

            if (FailedMessage != null)
            {
                builder.AppendFormat("| FailedMessage:\n #| {0}\n", FailedMessage.ToString());
            }

            builder.AppendFormat("+[/{0}]", this.GetType().Name);

            return builder.ToString();
        }

    }
}
