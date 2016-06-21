using System;
using System.Linq;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvTable : NetSimTable
    {
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override NetSimTableEntry GetRouteFor(string destinationId)
        {
            throw new NotImplementedException();
        }
    }
}