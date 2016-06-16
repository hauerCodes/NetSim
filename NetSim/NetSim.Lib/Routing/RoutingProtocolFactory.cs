using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Routing.DSDV;
using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing
{
    public static class RoutingProtocolFactory
    {
        public static NetSimRoutingProtocol CreateInstance(NetSimProtocolType protocolType, NetSimClient client)
        {
            switch (protocolType)
            {
                case NetSimProtocolType.AODV:
                    //TODO
                    return null;
                case NetSimProtocolType.DSDV:
                    return new DsdvRoutingProtocol(client);
                case NetSimProtocolType.DSR:
                    //TODO
                    return null;
                case NetSimProtocolType.OLSR:
                    //TODO
                    return null;
            }

            return null;
        }
    }
}
