using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSR
{
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
        /// Gets the next hop.
        /// </summary>
        /// <param name="currentNodeId">The current node identifier.</param>
        /// <returns></returns>
        public string GetNextHop(string currentNodeId)
        {
            if (Route == null || Route.Count == 0)
            {
                return null;
            }

            int searchedIndex = 0;

            searchedIndex = Route.IndexOf(currentNodeId) + 1;

            if (searchedIndex < 0)
            {
                return null;
            }
            return Route[searchedIndex];
        }

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
                Route = new List<string>(Route),
                Data = (NetSimMessage)Data.Clone()
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
            return $"[{this.GetType().Name} ({Sender} -> {Receiver})]"
                   + $"\n{Data}\n[/{this.GetType().Name}]";
        }
    }
}
