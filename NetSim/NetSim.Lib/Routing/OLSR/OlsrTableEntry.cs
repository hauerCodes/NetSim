using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new OlsrTableEntry()
            {
                Destination = this.Destination,
                Metric = this.Metric,
                NextHop = this.NextHop
            };
        }
    }
}
