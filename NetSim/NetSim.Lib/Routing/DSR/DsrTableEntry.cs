using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrTableEntry"/> class.
        /// </summary>
        public DsrTableEntry()
        {
            this.Route = new List<string>();
        }

        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        public List<string> Route { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new DsrTableEntry() { Destination = Destination, Route = new List<string>(this.Route) };
        }
    }
}
