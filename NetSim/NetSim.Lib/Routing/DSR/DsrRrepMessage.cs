using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrRrepMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrRrepMessage"/> class.
        /// </summary>
        public DsrRrepMessage()
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
            return new DsrRrepMessage()
            {
                Sender = this.Sender,
                Receiver = this.Receiver,
                Route = new List<string>(this.Route)
            };
        }
    }
}
