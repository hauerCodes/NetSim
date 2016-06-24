using System;
using System.Linq;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvTable : NetSimTable
    {

        public override object Clone()
        {
            return new AodvTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
        }

        public override NetSimTableEntry GetRouteFor(string destinationId)
        {
            return Entries.FirstOrDefault(e => e.Destination.Equals(destinationId));
        }
    }
}