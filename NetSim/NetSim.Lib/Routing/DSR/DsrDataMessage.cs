using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrDataMessage : DataMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrDataMessage"/> class.
        /// </summary>
        public DsrDataMessage()
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
    }
}
